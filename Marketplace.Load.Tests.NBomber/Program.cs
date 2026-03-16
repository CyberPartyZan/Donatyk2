using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NBomber.Contracts;
using NBomber.CSharp;

var baseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "https://localhost:7077";
var authToken = Environment.GetEnvironmentVariable("AUTH_TOKEN");
var testWrite = (Environment.GetEnvironmentVariable("TEST_WRITE") ?? "")
    .Equals("true", StringComparison.OrdinalIgnoreCase);

var httpClient = CreateHttpClient();
var setup = await InitializeAsync(httpClient, baseUrl, testWrite, authToken);

var scenario = Scenario
    .Create("lots_read", async context =>
    {
        var getLots = await Step.Run("get_lots", context, async () =>
        {
            var response = await httpClient.GetAsync($"{baseUrl}/api/lots");
            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: response.StatusCode.ToString())
                : Response.Fail(statusCode: response.StatusCode.ToString());
        });

        if (getLots.IsError) return getLots;

        var getLot = await Step.Run("get_lot", context, async () =>
        {
            if (setup.FirstLotId is null)
            {
                return Response.Ok();
            }

            var response = await httpClient.GetAsync($"{baseUrl}/api/lots/{setup.FirstLotId}");
            return response.IsSuccessStatusCode
                ? Response.Ok(statusCode: response.StatusCode.ToString())
                : Response.Fail(statusCode: response.StatusCode.ToString());
        });

        if (getLot.IsError) return getLot;

        var createDelete = await Step.Run("create_delete_lot", context, async () =>
        {
            if (!testWrite || string.IsNullOrWhiteSpace(authToken))
            {
                return Response.Ok();
            }

            var payload = CreateLotPayload(setup.Seller, setup.Category);
            using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/lots")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var createResponse = await httpClient.SendAsync(createRequest);
            if (createResponse.StatusCode != HttpStatusCode.Created)
            {
                return Response.Fail(statusCode: createResponse.StatusCode.ToString());
            }

            var location = createResponse.Headers.Location?.ToString() ?? string.Empty;
            var createdId = location.Split('/').LastOrDefault();

            if (!string.IsNullOrWhiteSpace(createdId))
            {
                using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/api/lots/{createdId}");
                deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

                var deleteResponse = await httpClient.SendAsync(deleteRequest);
                if (deleteResponse.StatusCode != HttpStatusCode.NoContent)
                {
                    return Response.Fail(statusCode: deleteResponse.StatusCode.ToString());
                }
            }

            return Response.Ok();
        });

        return createDelete;
    })
    .WithLoadSimulations(
        Simulation.RampingConstant(100, TimeSpan.FromSeconds(30)),
        Simulation.KeepConstant(100, TimeSpan.FromMinutes(1)),
        Simulation.RampingConstant(0, TimeSpan.FromSeconds(30))
    );

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();

static HttpClient CreateHttpClient()
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    return new HttpClient(handler, disposeHandler: true);
}

static async Task<SetupData> InitializeAsync(HttpClient httpClient, string baseUrl, bool testWrite, string? authToken)
{
    var firstLotId = await GetFirstLotIdAsync(httpClient, baseUrl);

    SellerData? seller = null;
    CategoryData? category = null;

    if (testWrite && !string.IsNullOrWhiteSpace(authToken))
    {
        seller = await GetOrCreateSellerAsync(httpClient, baseUrl, authToken);
        category = await GetOrCreateCategoryAsync(httpClient, baseUrl, authToken);
    }

    return new SetupData(firstLotId, seller, category);
}

static async Task<Guid?> GetFirstLotIdAsync(HttpClient httpClient, string baseUrl)
{
    var response = await httpClient.GetAsync($"{baseUrl}/api/lots");
    if (!response.IsSuccessStatusCode)
    {
        return null;
    }

    var content = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(content);

    if (doc.RootElement.ValueKind != JsonValueKind.Array)
    {
        return null;
    }

    foreach (var item in doc.RootElement.EnumerateArray())
    {
        var id = TryGetGuid(item, "id") ?? TryGetGuid(item, "Id");
        if (id is not null)
        {
            return id;
        }
    }

    return null;
}

static async Task<SellerData?> GetOrCreateSellerAsync(HttpClient httpClient, string baseUrl, string authToken)
{
    const string name = "k6 seller";
    const string description = "k6 perf seller";
    const string email = "k6@example.com";
    const string phoneNumber = "+380501234567";
    const string avatarImageUrl = "";

    var searchResponse = await httpClient.GetAsync($"{baseUrl}/api/sellers?search={Uri.EscapeDataString(name)}");
    if (searchResponse.IsSuccessStatusCode)
    {
        var existing = await FindSellerByNameAsync(searchResponse, name);
        if (existing is not null)
        {
            return existing with
            {
                Description = description,
                Email = email,
                PhoneNumber = phoneNumber,
                AvatarImageUrl = avatarImageUrl
            };
        }
    }

    using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/sellers")
    {
        Content = new StringContent(JsonSerializer.Serialize(new
        {
            id = Guid.Empty,
            name,
            description,
            email,
            phoneNumber,
            avatarImageUrl
        }), Encoding.UTF8, "application/json")
    };
    createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

    var createResponse = await httpClient.SendAsync(createRequest);
    if (!createResponse.IsSuccessStatusCode)
    {
        return null;
    }

    var retryResponse = await httpClient.GetAsync($"{baseUrl}/api/sellers?search={Uri.EscapeDataString(name)}");
    if (!retryResponse.IsSuccessStatusCode)
    {
        return null;
    }

    return await FindSellerByNameAsync(retryResponse, name);
}

static async Task<CategoryData?> GetOrCreateCategoryAsync(HttpClient httpClient, string baseUrl, string authToken)
{
    const string name = "k6 category";
    const string description = "k6 perf category";

    var listResponse = await httpClient.GetAsync($"{baseUrl}/api/categories");
    if (listResponse.IsSuccessStatusCode)
    {
        var existing = await FindCategoryByNameAsync(listResponse, name);
        if (existing is not null)
        {
            return existing with { Description = description };
        }
    }

    using var createRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/categories")
    {
        Content = new StringContent(JsonSerializer.Serialize(new
        {
            id = Guid.Empty,
            name,
            description,
            parentId = (Guid?)null,
            subCategories = Array.Empty<object>()
        }), Encoding.UTF8, "application/json")
    };
    createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

    var createResponse = await httpClient.SendAsync(createRequest);
    if (createResponse.StatusCode != HttpStatusCode.Created)
    {
        return null;
    }

    var content = await createResponse.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(content);
    var id = TryGetGuid(doc.RootElement, "id") ?? TryGetGuid(doc.RootElement, "Id");

    return id is null
        ? null
        : new CategoryData(id.Value, name, description, null);
}

static async Task<SellerData?> FindSellerByNameAsync(HttpResponseMessage response, string name)
{
    var content = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(content);

    if (doc.RootElement.ValueKind != JsonValueKind.Array)
    {
        return null;
    }

    var target = name.ToLowerInvariant();

    foreach (var item in doc.RootElement.EnumerateArray())
    {
        var candidate = TryGetString(item, "name") ?? TryGetString(item, "Name");
        if (candidate is not null && candidate.ToLowerInvariant() == target)
        {
            var id = TryGetGuid(item, "id") ?? TryGetGuid(item, "Id");
            if (id is null) return null;

            return new SellerData(
                id.Value,
                candidate,
                TryGetString(item, "description") ?? TryGetString(item, "Description") ?? string.Empty,
                TryGetString(item, "email") ?? TryGetString(item, "Email") ?? string.Empty,
                TryGetString(item, "phoneNumber") ?? TryGetString(item, "PhoneNumber") ?? string.Empty,
                TryGetString(item, "avatarImageUrl") ?? TryGetString(item, "AvatarImageUrl") ?? string.Empty
            );
        }
    }

    return null;
}

static async Task<CategoryData?> FindCategoryByNameAsync(HttpResponseMessage response, string name)
{
    var content = await response.Content.ReadAsStringAsync();
    using var doc = JsonDocument.Parse(content);

    if (doc.RootElement.ValueKind != JsonValueKind.Array)
    {
        return null;
    }

    var target = name.ToLowerInvariant();

    foreach (var item in doc.RootElement.EnumerateArray())
    {
        var candidate = TryGetString(item, "name") ?? TryGetString(item, "Name");
        if (candidate is not null && candidate.ToLowerInvariant() == target)
        {
            var id = TryGetGuid(item, "id") ?? TryGetGuid(item, "Id");
            if (id is null) return null;

            var parentId = TryGetGuid(item, "parentId") ?? TryGetGuid(item, "ParentId");

            return new CategoryData(
                id.Value,
                candidate,
                TryGetString(item, "description") ?? TryGetString(item, "Description") ?? string.Empty,
                parentId
            );
        }
    }

    return null;
}

static Guid? TryGetGuid(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var prop))
    {
        return null;
    }

    return prop.ValueKind switch
    {
        JsonValueKind.String when Guid.TryParse(prop.GetString(), out var id) => id,
        JsonValueKind.Number when Guid.TryParse(prop.GetRawText(), out var id) => id,
        _ => null
    };
}

static string? TryGetString(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var prop))
    {
        return null;
    }

    return prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.GetRawText();
}

static object CreateLotPayload(SellerData? seller, CategoryData? category)
{
    var sellerId = seller?.Id ?? Guid.Empty;
    var categoryId = category?.Id ?? Guid.Empty;

    return new
    {
        id = Guid.Empty,
        name = "k6 lot",
        description = "k6 performance test lot",
        price = new { amount = 10, currency = 0 },
        compensation = new { amount = 1, currency = 0 },
        stockCount = 1,
        discountedPrice = (object?)null,
        discount = 0,
        type = 0,
        stage = 1,
        seller = new
        {
            id = sellerId,
            name = seller?.Name ?? "k6 seller",
            description = seller?.Description ?? "k6 perf seller",
            email = seller?.Email ?? "k6@example.com",
            phoneNumber = seller?.PhoneNumber ?? "+380501234567",
            avatarImageUrl = seller?.AvatarImageUrl ?? string.Empty
        },
        category = new
        {
            id = categoryId,
            name = category?.Name ?? "k6 category",
            description = category?.Description ?? "k6 perf category",
            parentId = category?.ParentId,
            subCategories = Array.Empty<object>()
        },
        isActive = true,
        isCompensationPaid = false
    };
}

record SetupData(Guid? FirstLotId, SellerData? Seller, CategoryData? Category);

record SellerData(
    Guid Id,
    string Name,
    string Description,
    string Email,
    string PhoneNumber,
    string AvatarImageUrl);

record CategoryData(
    Guid Id,
    string Name,
    string Description,
    Guid? ParentId);