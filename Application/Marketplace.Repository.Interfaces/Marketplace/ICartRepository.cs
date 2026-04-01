namespace Marketplace.Repository
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByUserId(Guid userId);
        Task<Guid> AddItem(CartItem item);
        Task ChangeQuantity(Guid lotId, int quantity, Guid userId);
        Task RemoveItem(Guid lotId, Guid userId);
        Task ClearCart(Guid userId);
    }
}