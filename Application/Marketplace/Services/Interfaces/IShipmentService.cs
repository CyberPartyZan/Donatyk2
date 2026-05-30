namespace Marketplace
{
    public interface IShipmentService
    {
        Task<Guid> CreateShipmentAsync(Guid orderId, string shippingReference);
        Task TakeIntoProcessingAsync(Guid shipmentId);
        Task MarkShippedAsync(Guid shipmentId);
        Task MarkInTransitAsync(Guid shipmentId);
        Task MarkOutForDeliveryAsync(Guid shipmentId);
        Task MarkDeliveredAsync(Guid shipmentId);
    }
}