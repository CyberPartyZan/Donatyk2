using JsonSubTypes;
using MediatR;
using Newtonsoft.Json;

namespace Marketplace.Abstractions
{
    [JsonConverter(typeof(JsonSubtypes), nameof(Type))]
    [JsonSubtypes.KnownSubTypeAttribute(typeof(OrderCreated), nameof(OrderCreated))]
    public abstract record MarketplaceEvent : INotification
    {
        public string Type => GetType().Name;
    }
}
