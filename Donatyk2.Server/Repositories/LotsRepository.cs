using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;
using Donatyk2.Server.Enums;
using Donatyk2.Server.Models;
using Donatyk2.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Donatyk2.Server.Repositories
{
    public class LotsRepository : ILotsRepository
    {
        private readonly DonatykDbContext _db;

        public LotsRepository(DonatykDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Lot>> GetAll(LotSearchQuery query)
        {
            var q = _db.Lots
                .Include(l => l.Seller)
                .Where(l => !l.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query?.SearchText))
            {
                q = q.Where(e => e.Name.Contains(query.SearchText) || e.Description.Contains(query.SearchText));
            }

            if (query?.MinPrice is not null)
            {
                q = q.Where(e => e.Price.Amount >= (decimal)query.MinPrice.Value);
            }

            if (query?.MaxPrice is not null)
            {
                q = q.Where(e => e.Price.Amount <= (decimal)query.MaxPrice.Value);
            }

            if (query?.SellerId is not null)
            {
                q = q.Where(e => e.Seller != null && e.Seller.Id == query.SellerId.Value);
            }

            if (query?.Type is not null)
            {
                q = q.Where(e => e.Type == query.Type.Value);
            }

            if (query?.MinDiscount is not null)
            {
                q = q.Where(e => e.Discount >= query.MinDiscount.Value);
            }

            if (query?.MaxDiscount is not null)
            {
                q = q.Where(e => e.Discount <= query.MaxDiscount.Value);
            }

            var entities = await q
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return entities.Select(e => CreateFromEntity(e));
        }

        public async Task<Lot?> GetLotById(Guid id)
        {
            var entity = await _db.Lots
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            return entity is null ? null : CreateFromEntity(entity);
        }

        public async Task<Guid> CreateLot(Lot lot)
        {
            if (lot is null) throw new ArgumentNullException(nameof(lot));

            var entity = new LotEntity
            {
                Id = lot.Id == Guid.Empty ? Guid.NewGuid() : lot.Id,
                Name = lot.Name,
                Description = lot.Description,
                Price = lot.Price,
                Compensation = lot.Compensation,
                StockCount = lot.StockCount,
                Discount = lot.Discount,
                Type = lot.Type,
                Stage = lot.Stage,
                Seller = new SellerEntity
                {
                    Id = lot.Seller.Id == Guid.Empty ? Guid.NewGuid() : lot.Seller.Id,
                    Name = lot.Seller.Name,
                    Description = lot.Seller.Description,
                    Email = lot.Seller.Email,
                    PhoneNumber = lot.Seller.PhoneNumber,
                    AvatarImageUrl = lot.Seller.AvatarImageUrl ?? string.Empty,
                    UserId = lot.Seller.UserId,
                    CreatedAt = DateTime.UtcNow
                },
                IsActive = lot.IsActive,
                IsCompensationPaid = lot.IsCompensationPaid,
                CreatedAt = DateTime.UtcNow,
                EndOfAuction = (lot is AuctionLot a) ? a.EndOfAuction : null,
                AuctionStepPercent = (lot is AuctionLot a2) ? a2.AuctionStepPercent : null,
                TicketPrice = (lot is DrawLot dl) ? dl.TicketPrice : null,
                IsDeleted = false
            };

            _db.Lots.Add(entity);
            await _db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task UpdateLot(Guid id, Lot lot)
        {
            if (lot is null) throw new ArgumentNullException(nameof(lot));

            var existing = await _db.Lots
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (existing is null || existing.IsDeleted)
                throw new KeyNotFoundException($"Lot with id '{id}' not found.");

            existing.Name = lot.Name;
            existing.Description = lot.Description;
            existing.Price = lot.Price;
            existing.Compensation = lot.Compensation;
            existing.StockCount = lot.StockCount;
            existing.Discount = lot.Discount;
            existing.Type = lot.Type;
            existing.Stage = lot.Stage;
            existing.IsActive = lot.IsActive;
            existing.IsCompensationPaid = lot.IsCompensationPaid;
            existing.EndOfAuction = (lot is AuctionLot a) ? a.EndOfAuction : null;
            existing.AuctionStepPercent = (lot is AuctionLot a2) ? a2.AuctionStepPercent : null;
            existing.TicketPrice = (lot is DrawLot dl) ? dl.TicketPrice : existing.TicketPrice;

            if (lot.Seller is not null)
            {
                existing.Seller = new SellerEntity
                {
                    Id = lot.Seller.Id == Guid.Empty ? Guid.NewGuid() : lot.Seller.Id,
                    Name = lot.Seller.Name,
                    Description = lot.Seller.Description,
                    Email = lot.Seller.Email,
                    PhoneNumber = lot.Seller.PhoneNumber,
                    AvatarImageUrl = lot.Seller.AvatarImageUrl ?? string.Empty,
                    UserId = lot.Seller.UserId,
                    CreatedAt = existing.Seller?.CreatedAt ?? DateTime.UtcNow
                };
            }

            _db.Lots.Update(existing);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteLot(Guid id)
        {
            var existing = await _db.Lots.FindAsync(id);
            if (existing is null)
            {
                // nothing to do
                return;
            }

            if (!existing.IsDeleted)
            {
                existing.IsDeleted = true;
                _db.Lots.Update(existing);
                await _db.SaveChangesAsync();
            }
        }

        private static Lot CreateFromEntity(LotEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            var sellerEntity = entity.Seller ?? throw new InvalidOperationException("Lot entity must have a Seller.");

            return entity.Type switch
            {
                LotType.Simple => new Lot(
                    entity.Id,
                    entity.Name,
                    entity.Description,
                    entity.Price,
                    entity.Compensation,
                    entity.StockCount,
                    entity.Discount,
                    entity.Type,
                    entity.Stage,
                    new Seller(
                        sellerEntity.Id,
                        sellerEntity.Name,
                        sellerEntity.Description,
                        sellerEntity.Email,
                        sellerEntity.PhoneNumber,
                        sellerEntity.AvatarImageUrl,
                        sellerEntity.UserId),
                    entity.IsActive,
                    entity.IsCompensationPaid),

                LotType.Auction => new AuctionLot(
                    entity.Id,
                    entity.Name,
                    entity.Description,
                    entity.Price,
                    entity.Compensation,
                    entity.StockCount,
                    entity.Discount,
                    entity.Type,
                    entity.Stage,
                    new Seller(
                        sellerEntity.Id,
                        sellerEntity.Name,
                        sellerEntity.Description,
                        sellerEntity.Email,
                        sellerEntity.PhoneNumber,
                        sellerEntity.AvatarImageUrl,
                        sellerEntity.UserId),
                    entity.IsActive,
                    entity.IsCompensationPaid,
                    entity.EndOfAuction ?? throw new ArgumentNullException(nameof(entity.EndOfAuction)),
                    entity.AuctionStepPercent ?? throw new ArgumentNullException(nameof(entity.AuctionStepPercent))),

                LotType.Draw => new DrawLot(
                    entity.Id,
                    entity.Name,
                    entity.Description,
                    entity.Price,
                    entity.Compensation,
                    entity.StockCount,
                    entity.Discount,
                    entity.Type,
                    entity.Stage,
                    new Seller(
                        sellerEntity.Id,
                        sellerEntity.Name,
                        sellerEntity.Description,
                        sellerEntity.Email,
                        sellerEntity.PhoneNumber,
                        sellerEntity.AvatarImageUrl,
                        sellerEntity.UserId),
                    entity.IsActive,
                    entity.IsCompensationPaid,
                    entity.TicketPrice ?? throw new ArgumentNullException(nameof(entity.TicketPrice))),

                _ => throw new InvalidOperationException($"Unsupported LotType: {entity.Type}")
            };
        }
    }
}
