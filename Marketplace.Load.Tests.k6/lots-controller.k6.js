import http from "k6/http";
import { check, group, sleep } from "k6";
import { Trend } from "k6/metrics";

const baseUrl = __ENV.BASE_URL ?? "https://localhost:7077";
const authToken = __ENV.AUTH_TOKEN;
const testWrite = (__ENV.TEST_WRITE ?? "").toLowerCase() === "true";

const lotsListTime = new Trend("lots_list_time");
const lotsGetTime = new Trend("lots_get_time");
const lotsCreateTime = new Trend("lots_create_time");
const lotsDeleteTime = new Trend("lots_delete_time");

export const options = {
  insecureSkipTLSVerify: true,
  scenarios: {
    lots_read: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "30s", target: 10 },
        { duration: "1m", target: 10 },
        { duration: "30s", target: 0 },
      ],
      gracefulRampDown: "30s",
    },
  },
  thresholds: {
    http_req_failed: ["rate<0.01"],
    http_req_duration: ["p(95)<800"],
  },
};

function jsonHeaders(withAuth = false) {
  const headers = { "Content-Type": "application/json" };
  if (withAuth && authToken) {
    headers.Authorization = `Bearer ${authToken}`;
  }
  return { headers };
}

export function setup() {
  const res = http.get(`${baseUrl}/api/lots`);
  check(res, { "GET /api/lots is 200": (r) => r.status === 200 });

  let firstId = null;
  if (res.status === 200) {
    const data = res.json();
    if (Array.isArray(data) && data.length > 0) {
      firstId = data[0].id ?? data[0].Id ?? null;
    }
  }

  return { firstId };
}

export default function (data) {
  group("GET /api/lots", () => {
    const res = http.get(`${baseUrl}/api/lots`);
    lotsListTime.add(res.timings.duration);
    check(res, { "list is 200": (r) => r.status === 200 });
  });

  if (data.firstId) {
    group("GET /api/lots/{id}", () => {
      const res = http.get(`${baseUrl}/api/lots/${data.firstId}`);
      lotsGetTime.add(res.timings.duration);
      check(res, { "get is 200": (r) => r.status === 200 });
    });
  }

  if (testWrite && authToken) {
    group("POST + DELETE /api/lots", () => {
      const payload = createLotPayload();
      const createRes = http.post(
        `${baseUrl}/api/lots`,
        JSON.stringify(payload),
        jsonHeaders(true)
      );

      lotsCreateTime.add(createRes.timings.duration);
      check(createRes, { "create is 201": (r) => r.status === 201 });

      const location =
        createRes.headers.Location ?? createRes.headers.location ?? "";
      const createdId = location.split("/").pop();

      if (createdId) {
        const deleteRes = http.del(
          `${baseUrl}/api/lots/${createdId}`,
          null,
          jsonHeaders(true)
        );

        lotsDeleteTime.add(deleteRes.timings.duration);
        check(deleteRes, { "delete is 204": (r) => r.status === 204 });
      }
    });
  }

  sleep(1);
}

function createLotPayload() {
  return {
    id: "00000000-0000-0000-0000-000000000000",
    name: "k6 lot",
    description: "k6 performance test lot",
    price: { amount: 10, currency: 0 },
    compensation: { amount: 1, currency: 0 },
    stockCount: 1,
    discountedPrice: null,
    discount: 0,
    type: 0,
    stage: 1,
    seller: {
      id: "00000000-0000-0000-0000-000000000000",
      name: "k6 seller",
      description: "k6 perf seller",
      email: "k6@example.com",
      phoneNumber: "+380501234567",
      avatarImageUrl: "",
    },
    isActive: true,
    isCompensationPaid: false,
  };
}