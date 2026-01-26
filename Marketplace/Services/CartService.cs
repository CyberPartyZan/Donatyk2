using System.Security.Claims;
using Donatyk2.Server.Models;
using Donatyk2.Server.Repositories.Interfaces;
using Donatyk2.Server.Services.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Donatyk2.Server.Services
{
    public class CartService : ICartService
    {
        private readonly ClaimsPrincipal _user;
        private readonly ICartRepository _cartRepository;
        private readonly ILotsRepository _lotsRepository;

        public CartService(ClaimsPrincipal user, ICartRepository cartRepository, ILotsRepository lotsRepository)
        {
            _user = user;
            _cartRepository = cartRepository;
            _lotsRepository = lotsRepository;
        }

        public async Task<Cart> Get()
        {
            var userId = GetCurrentUserIdOrThrow();
            return await _cartRepository.GetCartByUserId(userId);
        }

        public async Task<Guid> AddItem(Guid lotId, int quantity)
        {
            if (quantity <= 0) 
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            var userId = GetCurrentUserIdOrThrow();

            var lot = await _lotsRepository.GetLotById(lotId);
            if (lot is null) 
                throw new KeyNotFoundException($"Lot with id '{lotId}' not found.");

            var item = new CartItem(
                lot: lot,
                quantity: quantity,
                userId: userId);

            return await _cartRepository.AddItem(item);
        }

        public async Task ChangeQuantity(Guid lotId, int quantity)
        {
            var userId = GetCurrentUserIdOrThrow();
            await _cartRepository.ChangeQuantity(lotId, quantity, userId);
        }

        public async Task RemoveItem(Guid lotId)
        {
            var userId = GetCurrentUserIdOrThrow();
            await _cartRepository.RemoveItem(lotId, userId);
        }

        private Guid GetCurrentUserIdOrThrow()
        {
            var sub = _user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(sub)) 
                throw new InvalidOperationException("User id is not available in the current principal.");

            return Guid.Parse(sub);
        }
    }
}