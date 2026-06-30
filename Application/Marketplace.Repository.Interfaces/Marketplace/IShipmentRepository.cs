namespace Marketplace.Repository
{
    public interface IShipmentRepository
    {
        Task<Guid> Create(Shipment shipment);
        Task<Shipment?> GetById(Guid shipmentId);
        Task Update(Shipment shipment);

        Task<IEnumerable<ShipmentAdminDto>> GetAll(string? search, int page, int pageSize, bool onlyPending, Guid? sellerId);
        Task<int> GetTotalCount(string? search, bool onlyPending, Guid? sellerId);
        Task<ShipmentStatisticsDto> GetStatistics(string? search, Guid? sellerId);
    }
}