namespace Marketplace
{
    public interface ICartService
    {
        Task<Cart> Get();
        Task<Guid> AddItem(Guid lotId, int quantity);
        Task ChangeQuantity(Guid lotId, int quantity);
        Task RemoveItem(Guid lotId);
    }
}