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

const defaultSeller = {
  name: "k6 seller",
  description: "k6 perf seller",
  email: "k6@example.com",
  phoneNumber: "+380501234567",
  avatarImageUrl: "",
};

const defaultCategory = {
  name: "k6 category",
  description: "k6 perf category",
  parentId: null,
  subCategories: [],
};

export const options = {
  insecureSkipTLSVerify: true,
  scenarios: {
    lots_read: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "30s", target: 100 },
        { duration: "1m", target: 100 },
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

  let seller = null;
  let category = null;

  if (testWrite && authToken) {
    seller = getOrCreateSeller(defaultSeller);
    category = getOrCreateCategory(defaultCategory);
  }

  return { firstId, seller, category };
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
      const payload = createLotPayload(data.seller, data.category);
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

function getOrCreateSeller(seed) {
  const searchRes = http.get(
    `${baseUrl}/api/sellers?search=${encodeURIComponent(seed.name)}`,
    jsonHeaders()
  );

  if (searchRes.status === 200) {
    const sellers = searchRes.json();
    const existing = findByName(sellers, seed.name);
    if (existing) {
      return normalizeSeller(existing, seed);
    }
  }

  const createRes = http.post(
    `${baseUrl}/api/sellers`,
    JSON.stringify({
      id: "00000000-0000-0000-0000-000000000000",
      name: seed.name,
      description: seed.description,
      email: seed.email,
      phoneNumber: seed.phoneNumber,
      avatarImageUrl: seed.avatarImageUrl,
    }),
    jsonHeaders(true)
  );

  check(createRes, { "create seller is 200": (r) => r.status === 200 });

  const retryRes = http.get(
    `${baseUrl}/api/sellers?search=${encodeURIComponent(seed.name)}`,
    jsonHeaders()
  );

  if (retryRes.status === 200) {
    const sellers = retryRes.json();
    const existing = findByName(sellers, seed.name);
    if (existing) {
      return normalizeSeller(existing, seed);
    }
  }

  return null;
}

function getOrCreateCategory(seed) {
  const listRes = http.get(`${baseUrl}/api/categories`, jsonHeaders());

  if (listRes.status === 200) {
    const categories = listRes.json();
    const existing = findByName(categories, seed.name);
    if (existing) {
      return normalizeCategory(existing, seed);
    }
  }

  const createRes = http.post(
    `${baseUrl}/api/categories`,
    JSON.stringify({
      id: "00000000-0000-0000-0000-000000000000",
      name: seed.name,
      description: seed.description,
      parentId: seed.parentId,
      subCategories: seed.subCategories,
    }),
    jsonHeaders(true)
  );

  check(createRes, { "create category is 201": (r) => r.status === 201 });

  if (createRes.status === 201) {
    const created = createRes.json();
    return normalizeCategory(created, seed);
  }

  return null;
}

function findByName(items, name) {
  if (!Array.isArray(items)) return null;
  const normalized = name.toLowerCase();
  return (
    items.find((x) => (x?.name ?? x?.Name ?? "").toLowerCase() === normalized) ??
    null
  );
}

function normalizeSeller(source, fallback) {
  return {
    id: source?.id ?? source?.Id ?? null,
    name: source?.name ?? source?.Name ?? fallback.name,
    description: source?.description ?? source?.Description ?? fallback.description,
    email: source?.email ?? source?.Email ?? fallback.email,
    phoneNumber: source?.phoneNumber ?? source?.PhoneNumber ?? fallback.phoneNumber,
    avatarImageUrl:
      source?.avatarImageUrl ?? source?.AvatarImageUrl ?? fallback.avatarImageUrl,
  };
}

function normalizeCategory(source, fallback) {
  return {
    id: source?.id ?? source?.Id ?? null,
    name: source?.name ?? source?.Name ?? fallback.name,
    description:
      source?.description ?? source?.Description ?? fallback.description,
    parentId: source?.parentId ?? source?.ParentId ?? fallback.parentId,
    subCategories:
      source?.subCategories ?? source?.SubCategories ?? fallback.subCategories,
  };
}

function createLotPayload(seller, category) {
  const sellerId = seller?.id ?? "00000000-0000-0000-0000-000000000000";
  const categoryId = category?.id ?? "00000000-0000-0000-0000-000000000000";

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
      id: sellerId,
      name: seller?.name ?? defaultSeller.name,
      description: seller?.description ?? defaultSeller.description,
      email: seller?.email ?? defaultSeller.email,
      phoneNumber: seller?.phoneNumber ?? defaultSeller.phoneNumber,
      avatarImageUrl: seller?.avatarImageUrl ?? defaultSeller.avatarImageUrl,
    },
    category: {
      id: categoryId,
      name: category?.name ?? defaultCategory.name,
      description: category?.description ?? defaultCategory.description,
      parentId: category?.parentId ?? defaultCategory.parentId,
      subCategories: category?.subCategories ?? defaultCategory.subCategories,
    },
    isActive: true,
    isCompensationPaid: false,
  };
}