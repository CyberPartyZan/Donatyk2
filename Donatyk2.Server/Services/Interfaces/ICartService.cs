using Donatyk2.Server.Models;

namespace Donatyk2.Server.Services.Interfaces
{
    public interface ICartService
    {
        Task<Cart> Get();
        Task<Guid> AddItem(Guid lotId, int quantity);
        Task ChangeQuantity(Guid lotId, int quantity);
        Task RemoveItem(Guid lotId);
    }
}