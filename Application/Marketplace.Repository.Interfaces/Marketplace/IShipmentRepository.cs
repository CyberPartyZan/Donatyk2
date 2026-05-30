namespace Marketplace.Repository
{
    public interface IShipmentRepository
    {
        Task<Guid> Create(Shipment shipment);
        Task<Shipment?> GetById(Guid shipmentId);
        Task Update(Shipment shipment);
    }
}