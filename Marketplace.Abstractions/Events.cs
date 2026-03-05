namespace Marketplace.Abstractions
{
    public record OrderCreated(Guid OrderId, Money Amount) : MarketplaceEvent;
}
