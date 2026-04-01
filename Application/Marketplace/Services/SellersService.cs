using Marketplace.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace
{
    public class SellersService : ISellersService
    {
        private readonly ISellersRepository _sellersRepository;
        private readonly ClaimsPrincipal _user;

        public SellersService(ClaimsPrincipal user, ISellersRepository sellersRepository)
        {
            _sellersRepository = sellersRepository;
            _user = user;
        }

        public async Task<IEnumerable<SellerDto>> GetAll(string search, int page, int pageSize)
        {
            var sellers = await _sellersRepository.GetAll(search, page, pageSize);
            return sellers.Select(s => new SellerDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                AvatarImageUrl = s.AvatarImageUrl
            });
        }

        public async Task<SellerDto?> GetById(Guid id)
        {
            var seller = await _sellersRepository.GetById(id);

            if (seller is null)
            {
                return null;
            }

            return new SellerDto
            {
                Id = seller.Id,
                Name = seller.Name,
                Description = seller.Description,
                Email = seller.Email,
                PhoneNumber = seller.PhoneNumber,
                AvatarImageUrl = seller.AvatarImageUrl
            };
        }

        public async Task<Guid> Create(SellerDto seller)
        {
            var userId = Guid.Parse(
                _user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            var newSeller = new Seller(
                id: Guid.NewGuid(),
                name: seller.Name,
                description: seller.Description,
                email: seller.Email,
                phoneNumber: seller.PhoneNumber,
                avatarImageUrl: seller.AvatarImageUrl ?? string.Empty,
                userId: userId);

            await _sellersRepository.Create(newSeller);

            return newSeller.Id;
        }

        public async Task Update(Guid id, SellerDto seller)
        {
            var existing = await _sellersRepository.GetById(id);
            if (existing is null)
            {
                throw new KeyNotFoundException($"Seller with id '{id}' not found.");
            }

            var updatedSeller = new Seller(
                id: id,
                name: seller.Name,
                description: seller.Description,
                email: seller.Email,
                phoneNumber: seller.PhoneNumber,
                avatarImageUrl: seller.AvatarImageUrl ?? existing.AvatarImageUrl,
                userId: existing.UserId);

            await _sellersRepository.Update(updatedSeller);
        }

        public async Task Delete(Guid id)
        {
            await _sellersRepository.Delete(id);
        }

        public async Task DeleteByUserId(Guid userId)
        {
            var seller = await _sellersRepository.GetByUserId(userId);
            if (seller is not null)
            {
                await Delete(seller.Id);
            }
        }
    }
}
