using Marketplace.BlobStorage;
using Marketplace.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Marketplace
{
    public class SellersService : ISellersService
    {
        private const string SellerAvatarPath = "sellers/avatars";

        private readonly ISellersRepository _sellersRepository;
        private readonly ClaimsPrincipal _user;
        private readonly IBlobStorageService _blobStorageService;

        public SellersService(
            ClaimsPrincipal user,
            ISellersRepository sellersRepository,
            IBlobStorageService blobStorageService)
        {
            _sellersRepository = sellersRepository;
            _user = user;
            _blobStorageService = blobStorageService;
        }

        public async Task<IEnumerable<SellerDto>> GetAll(string search, int page, int pageSize)
        {
            var sellers = await _sellersRepository.GetAll(search, page, pageSize);
            return sellers.Select(MapToDto);
        }

        public async Task<SellerDto?> GetById(Guid id)
        {
            var seller = await _sellersRepository.GetById(id);
            return seller is null ? null : MapToDto(seller);
        }

        public async Task<Guid> Create(SellerDto seller)
        {
            var userId = Guid.Parse(_user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            var newSeller = new Seller(
                id: Guid.NewGuid(),
                name: seller.Name,
                description: seller.Description,
                email: seller.Email,
                phoneNumber: seller.PhoneNumber,
                avatar: MapBlob(seller.Avatar),
                userId: userId);

            await _sellersRepository.Create(newSeller);
            return newSeller.Id;
        }

        public async Task Update(Guid id, SellerDto seller)
        {
            var existing = await _sellersRepository.GetById(id)
                ?? throw new KeyNotFoundException($"Seller with id '{id}' not found.");

            var updatedSeller = new Seller(
                id: id,
                name: seller.Name,
                description: seller.Description,
                email: seller.Email,
                phoneNumber: seller.PhoneNumber,
                avatar: MapBlob(seller.Avatar) ?? existing.Avatar,
                userId: existing.UserId);

            await _sellersRepository.Update(updatedSeller);
        }

        public Task Delete(Guid id) => _sellersRepository.Delete(id);

        public async Task DeleteByUserId(Guid userId)
        {
            var seller = await _sellersRepository.GetByUserId(userId);
            if (seller is not null) await Delete(seller.Id);
        }

        public async Task<BlobDto> UploadAvatar(Stream file, string fileName)
        {
            if (file is null || !file.CanRead)
                throw new ArgumentException("Avatar file is required.", nameof(file));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Avatar filename is required.", nameof(fileName));

            var key = await _blobStorageService.UploadAsync(file, SellerAvatarPath);

            return new BlobDto
            {
                Id = Guid.NewGuid(),
                FilePath = SellerAvatarPath,
                Key = key,
                FileName = Path.GetFileName(fileName)
            };
        }

        private static SellerDto MapToDto(Seller s)
        {
            var avatarDto = s.Avatar is null
                ? null
                : new BlobDto
                {
                    Id = s.Avatar.Id,
                    FilePath = s.Avatar.FilePath,
                    Key = s.Avatar.Key,
                    FileName = s.Avatar.FileName
                };

            return new SellerDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Email = s.Email,
                PhoneNumber = s.PhoneNumber,
                Avatar = avatarDto
            };
        }

        private static Blob? MapBlob(BlobDto? dto)
        {
            if (dto is null) return null;
            if (string.IsNullOrWhiteSpace(dto.FilePath) || string.IsNullOrWhiteSpace(dto.Key) || string.IsNullOrWhiteSpace(dto.FileName))
                return null;

            return new Blob(dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id, dto.FilePath!, dto.Key!, dto.FileName!);
        }
    }
}
