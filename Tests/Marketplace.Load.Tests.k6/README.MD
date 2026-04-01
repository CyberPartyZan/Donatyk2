winget install k6 --source winget
or
choco install k6

k6 run .\Marketplace.Load.Tests.k6\lots-controller.k6.js -e BASE_URL=https://localhost:7077 -e AUTH_TOKEN="YOUR_JWT" -e TEST_WRITE=true
or
& "C:\Program Files\k6\k6.exe" run .\Marketplace.Load.Tests.k6\lots-controller.k6.js -e BASE_URL=https://localhost:7077 -e AUTH_TOKEN="YOUR_JWT" -e TEST_WRITE=true
