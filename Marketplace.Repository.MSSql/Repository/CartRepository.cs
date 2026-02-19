using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class CartRepository : ICartRepository
    {
        private readonly DonatykDbContext _db;

        public CartRepository(DonatykDbContext db)
        {
            _db = db;
        }

        public async Task<Cart> GetCartByUserId(Guid userId)
        {
            var entities = await _db.CartItems
                .AsNoTracking()
                .Include(c => c.Lot)
                    .ThenInclude(l => l.Seller)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var items = entities
                .Select(e => new CartItem(
                    lot: CreateLotFromEntity(e.Lot),
                    quantity: e.Quantity,
                    userId: e.UserId))
                .ToList();

            return new Cart(items);
        }

        public async Task<Guid> AddItem(CartItem item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var existing = await _db.CartItems
                .FirstOrDefaultAsync(c => c.UserId == item.UserId && c.LotId == item.Lot.Id);

            if (existing is not null)
            {
                existing.Quantity += item.Quantity;
                _db.CartItems.Update(existing);
                await _db.SaveChangesAsync();
                return existing.LotId;
            }

            var lotExists = await _db.Lots
                .AnyAsync(l => l.Id == item.Lot.Id && !l.IsDeleted);

            if (!lotExists)
            {
                throw new KeyNotFoundException($"Lot with id '{item.Lot.Id}' not found.");
            }

            var entity = new CartItemEntity
            {
                LotId = item.Lot.Id,
                Quantity = item.Quantity,
                UserId = item.UserId
            };

            _db.CartItems.Add(entity);
            await _db.SaveChangesAsync();

            return entity.LotId;
        }

        public async Task ChangeQuantity(Guid lotId, int quantity, Guid userId)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
            }

            var existing = await _db.CartItems
                .FirstOrDefaultAsync(c => c.LotId == lotId && c.UserId == userId);

            if (existing is null)
            {
                throw new KeyNotFoundException($"Cart item for lot '{lotId}' not found for this user.");
            }

            existing.Quantity = quantity;
            _db.CartItems.Update(existing);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveItem(Guid lotId, Guid userId)
        {
            var existing = await _db.CartItems
                .FirstOrDefaultAsync(c => c.LotId == lotId && c.UserId == userId);

            if (existing is null)
            {
                throw new KeyNotFoundException($"Cart item for lot '{lotId}' not found for this user.");
            }

            _db.CartItems.Remove(existing);
            await _db.SaveChangesAsync();
        }

        public async Task ClearCart(Guid userId)
        {
            var items = _db.CartItems.Where(c => c.UserId == userId);
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }

        private static Lot CreateLotFromEntity(LotEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

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