namespace Marketplace
{
    public interface IShipmentService
    {
        Task<Guid> CreateShipmentAsync(Guid orderId, string trackingNumber);
        Task TakeIntoProcessingAsync(Guid shipmentId, string trackingNumber);
        Task MarkShippedAsync(Guid shipmentId);
        Task MarkInTransitAsync(Guid shipmentId);
        Task MarkOutForDeliveryAsync(Guid shipmentId);
        Task MarkDeliveredAsync(Guid shipmentId);

        Task<IEnumerable<ShipmentAdminDto>> GetAllAsync(string? search, int page, int pageSize, bool onlyPending, Guid? sellerId);
        Task<int> GetTotalCountAsync(string? search, bool onlyPending, Guid? sellerId);
        Task<ShipmentStatisticsDto> GetStatisticsAsync(string? search, Guid? sellerId);
    }
}