using Marketplace.Repository.MSSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Integration.Tests;

internal static class IntegrationTestsHelper
{
    // ── User helpers ─────────────────────────────────────────────────────────

    public static async Task<ApplicationUser> EnsureUserExistsAsync(
        IServiceProvider services,
        Guid userId,
        string email,
        string rawPassword,
        string appPasswordValue)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var existing = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (existing is not null)
            return existing;

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550000",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = appPasswordValue
        };

        var result = await userManager.CreateAsync(user, rawPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create test user: {errors}");
        }

        return user;
    }

    public static async Task<ApplicationUser> CreateUserAsync(
        IServiceProvider services,
        string defaultPassword,
        bool emailConfirmed = true,
        string? email = null,
        Guid? userId = null)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        email ??= $"user-{Guid.NewGuid():N}@example.com";

        var user = new ApplicationUser
        {
            Id = userId ?? Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = emailConfirmed,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550000",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = defaultPassword
        };

        var result = await userManager.CreateAsync(user, defaultPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create test user: {errors}");
        }

        if (emailConfirmed && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }
        else if (!emailConfirmed && user.EmailConfirmed)
        {
            user.EmailConfirmed = false;
            await userManager.UpdateAsync(user);
        }

        return user;
    }

    public static async Task<ApplicationUser> SeedUserAsync(
        IServiceProvider services,
        Action<ApplicationUser>? configure = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var email = $"user-{Guid.NewGuid():N}@example.com";

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = "integration-user",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550000",
            PhoneNumberConfirmed = true,
            LockoutEnabled = false,
            CreatedAt = DateTime.UtcNow,
            Password = "integration-user"
        };

        configure?.Invoke(user);

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    // ── Category helpers ──────────────────────────────────────────────────────

    public static async Task<CategoryEntity> SeedCategoryAsync(
        IServiceProvider services,
        string? name = null,
        Guid? parentId = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var unique = Guid.NewGuid().ToString("N");
        var entity = new CategoryEntity
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Category {unique}",
            Description = $"Description for {(name ?? "category")} {unique}",
            ParentCategoryId = parentId
        };

        db.Categories.Add(entity);
        await db.SaveChangesAsync();
        return entity;
    }

    public static async Task<(CategoryEntity Parent, CategoryEntity Child)> SeedCategoryHierarchyAsync(
        IServiceProvider services)
    {
        var parent = await SeedCategoryAsync(services, "Parent Category");
        var child = await SeedCategoryAsync(services, "Child Category", parent.Id);
        return (parent, child);
    }

    // ── Seller helpers ────────────────────────────────────────────────────────

    public static async Task<SellerEntity> SeedSellerAsync(
        IServiceProvider services,
        Action<SellerEntity>? configure = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var sellerUser = BuildSellerUser();

        var avatar = new BlobEntity
        {
            Id = Guid.NewGuid(),
            FilePath = "sellers/avatars",
            Key = Guid.NewGuid().ToString("N"),
            FileName = "seed-avatar.png"
        };

        var seller = new SellerEntity
        {
            Id = Guid.NewGuid(),
            Name = "Seed Seller",
            Description = "Seller seeded for integration tests.",
            Email = $"seed-seller-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+15555550001",
            AvatarId = avatar.Id,
            Avatar = avatar,
            UserId = sellerUser.Id,
            User = sellerUser,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        configure?.Invoke(seller);

        db.Users.Add(sellerUser);

        if (seller.Avatar is not null)
        {
            var exists = await db.Blobs.AnyAsync(b => b.Id == seller.Avatar.Id);
            if (!exists)
                db.Blobs.Add(seller.Avatar);
        }

        db.Sellers.Add(seller);
        await db.SaveChangesAsync();
        return seller;
    }

    // ── Lot helpers ───────────────────────────────────────────────────────────

    public static async Task<LotEntity> SeedLotAsync(
        IServiceProvider services,
        int stockCount = 10,
        LotType type = LotType.Simple,
        LotStage stage = LotStage.PendingApproval,
        SellerEntity? sellerEntry = null,
        Action<LotEntity>? configure = null)
    {
        var seller = sellerEntry ?? await SeedSellerAsync(services);
        var category = await SeedCategoryAsync(services);

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var trackedSeller = await db.Sellers.Include(s => s.User).SingleAsync(s => s.Id == seller.Id);
        var trackedCategory = await db.Categories.SingleAsync(c => c.Id == category.Id);

        var lot = new LotEntity
        {
            Id = Guid.NewGuid(),
            Name = "Existing lot",
            Description = "This is an existing lot.",
            Price = new Money(100, Currency.USD),
            Compensation = new Money(50, Currency.USD),
            StockCount = stockCount,
            DiscountedPrice = null,
            Type = type,
            Stage = stage,
            Seller = trackedSeller,
            SellerId = trackedSeller.Id,
            Category = trackedCategory,
            IsActive = true,
            IsCompensationPaid = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            EndOfAuction = type == LotType.Auction ? DateTime.UtcNow.AddHours(2) : null,
            AuctionStepPercent = type == LotType.Auction ? 5 : null,
            TicketPrice = type == LotType.Draw ? new Money(5, Currency.USD) : null,
            TicketsSold = type == LotType.Draw ? 0 : null,
            IsDrawn = false
        };

        configure?.Invoke(lot);

        db.Lots.Add(lot);
        await db.SaveChangesAsync();

        return lot;
    }

    // ── Cart helpers ──────────────────────────────────────────────────────────

    public static async Task<CartItemEntity> SeedCartItemAsync(
        IServiceProvider services,
        Guid lotId,
        int quantity,
        Guid userId)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var item = new CartItemEntity
        {
            LotId = lotId,
            Quantity = quantity,
            UserId = userId
        };

        db.CartItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    // ── Order helpers ─────────────────────────────────────────────────────────

    public static async Task<OrderEntity> SeedOrderAsync(
        IServiceProvider services,
        Guid customerId,
        OrderStatus status = OrderStatus.Paid,
        Action<OrderEntity>? configure = null)
    {
        var lot = await SeedLotAsync(services, stockCount: 10, type: LotType.Simple, stage: LotStage.Approved);

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = status,
            Total = new Money(100m, Currency.USD),
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = new ShippingAddressEntity
            {
                Id = Guid.NewGuid(),
                RecipientName = "Test Buyer",
                Line1 = "123 Test Street",
                Line2 = null,
                City = "Testville",
                State = "TS",
                PostalCode = "12345",
                Country = "US",
                Phone = "+15555551234"
            },
            PaymentProvider = "Stripe",
            PaymentTaxRate = 0.07m,
            PaymentReturnUrl = "https://example.com/return",
            PaymentReference = $"PAY-{Guid.NewGuid():N}",
            Items =
            [
                new OrderItemEntity
                {
                    LotId = lot.Id,
                    NameSnapshot = lot.Name,
                    UnitPrice = new Money(100m, Currency.USD),
                    Quantity = 1
                }
            ]
        };

        configure?.Invoke(order);

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    // ── Shipment helpers ──────────────────────────────────────────────────────

    public static async Task<ShipmentEntity> SeedShipmentAsync(
        IServiceProvider services,
        Guid orderId,
        ShipmentStatus status = ShipmentStatus.Created,
        string? trackingNumber = null)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var shipment = new ShipmentEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            TrackingNumber = trackingNumber ?? $"SHIP-{orderId:N}",
            Status = status,
            ShippingAddress = new ShippingAddressEntity
            {
                Id = Guid.NewGuid(),
                RecipientName = "Test Buyer",
                Line1 = "123 Test Street",
                Line2 = null,
                City = "Testville",
                State = "TS",
                PostalCode = "12345",
                Country = "US",
                Phone = "+15555551234"
            },
            CreatedAt = DateTime.UtcNow
        };

        db.Shipments.Add(shipment);
        await db.SaveChangesAsync();
        return shipment;
    }

    // ── DeliveryPreferences helpers ───────────────────────────────────────────

    public static async Task<DeliveryPreferencesEntity> SeedDeliveryPreferencesAsync(
        IServiceProvider services,
        Guid userId,
        DeliveryCarrier carrier = DeliveryCarrier.UPS)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        var address = new ShippingAddressEntity
        {
            Id = Guid.NewGuid(),
            RecipientName = "Test Recipient",
            Line1 = "123 Test Street",
            Line2 = null,
            City = "Testville",
            State = "TS",
            PostalCode = "12345",
            Country = "US",
            Phone = "+15555551234"
        };

        var preference = new DeliveryPreferencesEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Carrier = carrier,
            ShippingAddress = address
        };

        db.DeliveryPreferences.Add(preference);
        await db.SaveChangesAsync();

        return preference;
    }

    // ── Private builders ──────────────────────────────────────────────────────

    private static ApplicationUser BuildSellerUser()
    {
        var email = $"seller-{Guid.NewGuid():N}@example.com";
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = "seller-password",
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N"),
            PhoneNumber = "+15555550000",
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            Password = "seller-password"
        };
    }
}
