namespace Marketplace.Abstractions
{
    public record OrderCreated(Guid OrderId, Money Amount) : MarketplaceEvent;
    public record PaymentProcessed(Guid OrderId, bool Succeeded) : MarketplaceEvent;
    public record ShipmentCreated(Guid OrderId, Guid ShipmentId, DateTime CreatedAt) : MarketplaceEvent;
    public record DrawLaunched(Guid LotId) : MarketplaceEvent;
    public record AuctionEnded(Guid LotId) : MarketplaceEvent;
}
