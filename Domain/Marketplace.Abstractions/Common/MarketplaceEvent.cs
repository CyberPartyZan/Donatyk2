using JsonSubTypes;
using MediatR;
using Newtonsoft.Json;

namespace Marketplace.Abstractions
{
    [JsonConverter(typeof(JsonSubtypes), nameof(Type))]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(OrderCreated), nameof(OrderCreated))]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(PaymentProcessed), nameof(PaymentProcessed))]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(ShipmentCreated), nameof(ShipmentCreated))]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(DrawLaunched), nameof(DrawLaunched))]
    public abstract record MarketplaceEvent : INotification
    {
        public string Type => GetType().Name;
    }
}
