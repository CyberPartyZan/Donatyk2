using System.Security.Claims;
using Marketplace.Cache;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Marketplace
{
    public class LotsService : ILotsService
    {
        private readonly ILotsRepository _lotsRepository;
        private readonly IDistributedCache _cache;
        private readonly ClaimsPrincipal _user;

        public LotsService(
            ClaimsPrincipal user,
            ILotsRepository lotsRepository,
            IDistributedCache cache)
        {
            _lotsRepository = lotsRepository;
            _cache = cache;
            _user = user;
        }

        public async Task<IEnumerable<LotDto>> GetAll(LotSearchQuery query)
        {
            var effectiveQuery = query ?? new LotSearchQuery();

            if (effectiveQuery.IsCacheSupported())
            {
                var cacheKey = effectiveQuery.ToCacheKey();
                if (_cache.TryGet(cacheKey, out IReadOnlyCollection<LotDto> cached))
                {
                    return cached;
                }
            }

            var lots = await _lotsRepository.GetAll(effectiveQuery);
            var results = lots.Select(ToDto).ToList();

            if (effectiveQuery.IsCacheSupported())
            {
                _cache.Set(effectiveQuery.ToCacheKey(), results);
            }

            return results;
        }

        public async Task<LotDto?> GetLotById(Guid id)
        {
            var cacheKey = $"lots:id:{id:N}";
            if (_cache.TryGet(cacheKey, out LotDto cached))
            {
                return cached;
            }

            var lot = await _lotsRepository.GetLotById(id);
            if (lot is null) return null;

            var dto = ToDto(lot);
            _cache.Set(cacheKey, dto);

            return dto;
        }

        public async Task<Guid> CreateLot(LotDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var userId = GetCurrentUserIdOrThrow();

            var seller = new Seller(
                dto.Seller.Id == Guid.Empty ? Guid.NewGuid() : dto.Seller.Id,
                dto.Seller.Name,
                dto.Seller.Description,
                dto.Seller.Email,
                dto.Seller.PhoneNumber,
                dto.Seller.AvatarImageUrl,
                userId);

            var lot = new Lot(
                id: dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                name: dto.Name,
                description: dto.Description,
                price: dto.Price,
                compensation: dto.Compensation,
                stockCount: dto.StockCount,
                discount: dto.Discount,
                type: dto.Type,
                stage: LotStage.PendingApproval,
                seller: seller,
                isActive: dto.IsActive,
                isCompensationPaid: dto.IsCompensationPaid);

            // child specific props are handled by repository/factory if needed (e.g. AuctionLot/DrawLot)
            return await _lotsRepository.CreateLot(lot);
        }

        public async Task UpdateLot(Guid id, LotDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            var existing = await _lotsRepository.GetLotById(id);
            if (existing is null) throw new KeyNotFoundException($"Lot with id '{id}' not found.");

            var sellerUserId = existing.Seller?.UserId ?? GetCurrentUserIdOrThrow();
            var seller = new Seller(
                dto.Seller.Id == Guid.Empty ? Guid.NewGuid() : dto.Seller.Id,
                dto.Seller.Name,
                dto.Seller.Description,
                dto.Seller.Email,
                dto.Seller.PhoneNumber,
                dto.Seller.AvatarImageUrl,
                sellerUserId);

            var updated = new Lot(
                id: id,
                name: dto.Name,
                description: dto.Description,
                price: dto.Price,
                compensation: dto.Compensation,
                stockCount: dto.StockCount,
                discount: dto.Discount,
                type: dto.Type,
                stage: dto.Stage,
                seller: seller,
                isActive: dto.IsActive,
                isCompensationPaid: dto.IsCompensationPaid);

            await _lotsRepository.UpdateLot(id, updated);
        }

        public async Task DeleteLot(Guid id)
        {
            await _lotsRepository.DeleteLot(id);
        }

        public async Task ApproveLot(Guid id)
        {
            // TODO: Remove double check for existence (GetLotById) and approval (Stage) - can be handled in repository with a single query and appropriate exception handling
            var existing = await _lotsRepository.GetLotById(id);
            if (existing is null) throw new KeyNotFoundException($"Lot with id '{id}' not found.");

            if (existing.Stage == LotStage.Approved)
            {
                return;
            }

            await _lotsRepository.ApproveLot(id);
        }

        public async Task DeclineLot(Guid id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Decline reason is required.", nameof(reason));
            }

            var existing = await _lotsRepository.GetLotById(id);
            if (existing is null) throw new KeyNotFoundException($"Lot with id '{id}' not found.");

            var trimmedReason = reason.Trim();
            if (existing.Stage == LotStage.Denied && string.Equals(existing.DeclineReason, trimmedReason, StringComparison.Ordinal))
            {
                return;
            }

            await _lotsRepository.DeclineLot(id, trimmedReason);
        }

        private Guid GetCurrentUserIdOrThrow()
        {
            var sub = _user.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (string.IsNullOrWhiteSpace(sub)) throw new InvalidOperationException("User id is not available in the current principal.");
            return Guid.Parse(sub);
        }

        private static LotDto ToDto(Lot lot)
        {
            return new LotDto
            {
                Id = lot.Id,
                Name = lot.Name,
                Description = lot.Description,
                Price = lot.Price,
                Compensation = lot.Compensation,
                StockCount = lot.StockCount,
                Discount = lot.Discount,
                Type = lot.Type,
                Stage = lot.Stage,
                DeclineReason = lot.DeclineReason,
                Seller = new SellerDto
                {
                    Id = lot.Seller.Id,
                    Name = lot.Seller.Name,
                    Description = lot.Seller.Description,
                    Email = lot.Seller.Email,
                    PhoneNumber = lot.Seller.PhoneNumber,
                    AvatarImageUrl = lot.Seller.AvatarImageUrl ?? string.Empty
                },
                IsActive = lot.IsActive,
                IsCompensationPaid = lot.IsCompensationPaid,
                CreatedAt = DateTime.UtcNow,
                EndOfAuction = lot is AuctionLot al ? al.EndOfAuction : null,
                AuctionStepPercent = lot is AuctionLot a2 ? a2.AuctionStepPercent : null,
                TicketPrice = lot is DrawLot dl ? dl.TicketPrice : null
            };
        }
    }
}
