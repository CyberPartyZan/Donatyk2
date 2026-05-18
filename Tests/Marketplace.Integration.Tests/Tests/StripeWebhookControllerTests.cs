using Marketplace.Repository.MSSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using Stripe.Checkout;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Marketplace.Integration.Tests;

/// <summary>
/// Integration tests for the Stripe webhook controller.
/// Stripe gateway is activated by overriding appsettings inside the factory.
/// The Stripe API itself is mocked via WireMock.Net so no real Stripe account is needed.
/// </summary>
public class StripeWebhookControllerTests : IClassFixture<StripeWebhookWebApplicationFactory>, IAsyncLifetime
{
    private readonly StripeWebhookWebApplicationFactory _factory;
    private HttpClient _client = default!;

    public StripeWebhookControllerTests(StripeWebhookWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(TestAuthHandler.Scheme);
        await _factory.EnsureTestUserAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Webhook_CheckoutSessionCompleted_MarksOrderPaid()
    {
        var (orderId, _) = await CreateSimpleOrderAsync();

        var payload = BuildCheckoutSessionEvent(
            orderId,
            eventType: "checkout.session.completed",
            paymentIntentId: "pi_test_001");

        var response = await PostWebhookAsync(payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var order = await db.Orders.SingleAsync(o => o.Id == orderId);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal("pi_test_001", order.PaymentReference);
    }

    [Fact]
    public async Task Webhook_CheckoutSessionExpired_CancelsOrder()
    {
        var (orderId, _) = await CreateSimpleOrderAsync();

        var payload = BuildCheckoutSessionEvent(
            orderId,
            eventType: "checkout.session.expired",
            paymentIntentId: null);

        var response = await PostWebhookAsync(payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        var order = await db.Orders.SingleAsync(o => o.Id == orderId);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task Webhook_InvalidSignature_ReturnsBadRequest()
    {
        var payload = BuildCheckoutSessionEvent(
            Guid.NewGuid(),
            eventType: "checkout.session.completed",
            paymentIntentId: "pi_test_bad");

        // tamper the signature
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/stripe/webhook")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Stripe-Signature", "t=invalid,v1=badsig");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Webhook_UnhandledEventType_ReturnsOkAndIsIgnored()
    {
        var payload = BuildGenericEvent("payment_intent.created");

        var response = await PostWebhookAsync(payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Webhook_MissingOrderIdInMetadata_ReturnsOk()
    {
        var payload = BuildCheckoutSessionEventWithoutOrderId("checkout.session.completed");

        var response = await PostWebhookAsync(payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── Stripe Checkout Session creation returns URL ──────────────────────────

    [Fact]
    public async Task CreatePaymentUrl_WithStripeGateway_ReturnsStripeCheckoutUrl()
    {
        var lot = await SeedLotAsync(type: LotType.Simple, stockCount: 5);
        await SeedCartItemAsync(lot.Id, quantity: 1);

        _factory.WireMockServer.Given(
                Request.Create().WithPath("/v1/checkout/sessions").UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(BuildStripeCheckoutSessionResponse(lot.Id)));

        var response = await _client.PostAsJsonAsync("/api/orders/checkout",
            CreateCheckoutRequest(provider: "Stripe"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("stripe.com/pay", response.Headers.Location!.ToString());
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<(Guid orderId, LotEntity lot)> CreateSimpleOrderAsync()
    {
        var lot = await SeedLotAsync(type: LotType.Simple, stockCount: 5);
        await SeedCartItemAsync(lot.Id, quantity: 1);

        _factory.WireMockServer.Given(
                Request.Create().WithPath("/v1/checkout/sessions").UsingPost())
            .RespondWith(
                Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(BuildStripeCheckoutSessionResponse(lot.Id)));

        var checkoutResponse = await _client.PostAsJsonAsync(
            "/api/orders/checkout",
            CreateCheckoutRequest(provider: "Stripe"));

        Assert.Equal(HttpStatusCode.Redirect, checkoutResponse.StatusCode);
        Assert.True(checkoutResponse.Headers.TryGetValues("X-Order-Id", out var vals));
        var orderId = Guid.Parse(vals!.Single());

        _factory.WireMockServer.ResetMappings();

        return (orderId, lot);
    }

    private async Task<HttpResponseMessage> PostWebhookAsync(string payload)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signature = ComputeStripeSignature(
            timestamp,
            payload,
            StripeWebhookWebApplicationFactory.TestWebhookSecret);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/stripe/webhook")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Stripe-Signature", $"t={timestamp},v1={signature}");
        return await _client.SendAsync(request);
    }

    private static string ComputeStripeSignature(long timestamp, string payload, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(
            Encoding.UTF8.GetBytes(secret));
        var data = Encoding.UTF8.GetBytes($"{timestamp}.{payload}");
        var hash = hmac.ComputeHash(data);
        return Convert.ToHexStringLower(hash);
    }

    private static string BuildCheckoutSessionEvent(
        Guid orderId,
        string eventType,
        string? paymentIntentId)
    {
        var paymentIntentJson = paymentIntentId is not null
            ? $"\"{paymentIntentId}\""
            : "null";

        return $$"""
        {
          "id": "evt_test_{{Guid.NewGuid():N}}",
          "object": "event",
          "type": "{{eventType}}",
          "api_version": "2024-06-20",
          "data": {
            "object": {
              "id": "cs_test_{{Guid.NewGuid():N}}",
              "object": "checkout.session",
              "payment_intent": {{paymentIntentJson}},
              "metadata": {
                "orderId": "{{orderId}}"
              },
              "status": "complete"
            }
          }
        }
        """;
    }

    private static string BuildCheckoutSessionEventWithoutOrderId(string eventType) =>
        $$"""
        {
          "id": "evt_test_{{Guid.NewGuid():N}}",
          "object": "event",
          "type": "{{eventType}}",
          "api_version": "2024-06-20",
          "data": {
            "object": {
              "id": "cs_test_{{Guid.NewGuid():N}}",
              "object": "checkout.session",
              "payment_intent": null,
              "metadata": {},
              "status": "complete"
            }
          }
        }
        """;

    private static string BuildGenericEvent(string eventType) =>
        $$"""
        {
          "id": "evt_test_{{Guid.NewGuid():N}}",
          "object": "event",
          "type": "{{eventType}}",
          "api_version": "2024-06-20",
          "data": {
            "object": {}
          }
        }
        """;

    private static string BuildStripeCheckoutSessionResponse(Guid lotId) =>
        $$"""
        {
          "id": "cs_test_{{Guid.NewGuid():N}}",
          "object": "checkout.session",
          "url": "https://stripe.com/pay/cs_test_abc",
          "payment_intent": "pi_test_{{Guid.NewGuid():N}}",
          "metadata": {
            "orderId": "{{lotId}}"
          },
          "status": "open"
        }
        """;

    private static CheckoutRequest CreateCheckoutRequest(string provider = "Stripe") =>
        new()
        {
            Shipping = new ShippingInfoDto
            {
                RecipientName = "Stripe Tester",
                Line1 = "1 Stripe Ave",
                City = "San Francisco",
                State = "CA",
                PostalCode = "94103",
                Country = "US",
                Phone = "+14155550100"
            },
            Payment = new PaymentInfoDto
            {
                Provider = provider,
                TaxRate = 0.07m,
                ReturnUrl = "https://example.com/return"
            }
        };

    private async Task SeedCartItemAsync(Guid lotId, int quantity)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        db.CartItems.Add(new CartItemEntity
        {
            LotId = lotId,
            Quantity = quantity,
            UserId = TestAuthHandler.UserId
        });
        await db.SaveChangesAsync();
    }

    private async Task<LotEntity> SeedLotAsync(LotType type, int stockCount)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var sellerUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"stripe-seller-{Guid.NewGuid():N}@test.com",
            NormalizedUserName = $"STRIPE-SELLER@TEST.COM",
            Email = $"stripe-seller-{Guid.NewGuid():N}@test.com",
            NormalizedEmail = $"STRIPE-SELLER@TEST.COM",
            EmailConfirmed = true,
            PasswordHash = "pw",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+10000000000",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = "pw"
        };

        var category = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = $"Stripe Cat {Guid.NewGuid():N}",
            Description = "Stripe test category"
        };

        var seller = new SellerEntity
        {
            Id = Guid.NewGuid(),
            Name = "Stripe Seller",
            Description = "Stripe test seller",
            Email = $"stripe-{Guid.NewGuid():N}@test.com",
            PhoneNumber = "+10000000001",
            AvatarImageUrl = string.Empty,
            UserId = sellerUser.Id,
            User = sellerUser,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var lot = new LotEntity
        {
            Id = Guid.NewGuid(),
            Name = $"Stripe Lot {Guid.NewGuid():N}".Substring(0, 16),
            Description = "Lot for Stripe integration tests.",
            Price = new Money(50m, Currency.USD),
            Compensation = new Money(30m, Currency.USD),
            StockCount = stockCount,
            DiscountedPrice = null,
            Type = type,
            Stage = LotStage.Approved,
            Seller = seller,
            Category = category,
            IsActive = true,
            IsCompensationPaid = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            IsDrawn = false
        };

        db.Users.Add(sellerUser);
        db.Categories.Add(category);
        db.Sellers.Add(seller);
        db.Lots.Add(lot);
        await db.SaveChangesAsync();
        return lot;
    }
}