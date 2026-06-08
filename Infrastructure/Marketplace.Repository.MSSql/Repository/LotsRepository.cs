using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal class LotsRepository : ILotsRepository
    {
        private readonly MarketplaceDbContext _db;

        public LotsRepository(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Lot>> GetAll(LotSearchQuery query)
        {
            var q = _db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.Characteristics)
                .Include(l => l.Images)
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

            if (query?.Stage is not null) // NEW
            {
                q = q.Where(e => e.Stage == query.Stage.Value);
            }

            if (query?.MinDiscount is not null)
            {
                q = q.Where(e =>
                    (e.Price.Amount == 0m
                        ? 0m
                        : ((e.Price.Amount - (e.DiscountedPrice != null ? e.DiscountedPrice.Amount : e.Price.Amount)) / e.Price.Amount) * 100m) >= query.MinDiscount.Value);
            }

            if (query?.MaxDiscount is not null)
            {
                q = q.Where(e =>
                    (e.Price.Amount == 0m
                        ? 0m
                        : ((e.Price.Amount - (e.DiscountedPrice != null ? e.DiscountedPrice.Amount : e.Price.Amount)) / e.Price.Amount) * 100m) <= query.MaxDiscount.Value);
            }

            if (query?.CategoryId is not null)
            {
                q = q.Where(e => e.Category != null && e.Category.Id == query.CategoryId.Value);
            }

            if (query?.GetDeleted is not null)
            {
                q = q.Where(e => e.IsDeleted == query.GetDeleted.Value);
            }
            else
            {
                q = q.Where(e => !e.IsDeleted && !e.Seller.IsDeleted);
            }

            if (query?.GetExhausted != true)
            {
                q = q.Where(e => e.StockCount > 0);
            }

            var pageNumber = query?.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query?.PageSize > 0 ? query.PageSize : 20;

            var entities = await q
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return entities.Select(e => CreateFromEntity(e));
        }

        public async Task<Lot?> GetLotById(Guid id)
        {
            var entity = await _db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.BidHistory)
                .Include(l => l.Characteristics)
                .Include(l => l.Images)
                .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

            return entity is null ? null : CreateFromEntity(entity);
        }

        public async Task<IEnumerable<AuctionLot>> GetEndedAuctionLots(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var entities = await _db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Category)
                .Include(l => l.BidHistory)
                .Include(l => l.Characteristics)
                .Include(l => l.Images)
                .Where(l => l.Type == LotType.Auction
                            && !l.IsDeleted
                            && l.IsActive
                            && l.EndOfAuction != null
                            && l.EndOfAuction <= now
                            && l.StockCount > 0)
                .ToListAsync(cancellationToken);

            return entities
                .Select(CreateFromEntity)
                .OfType<AuctionLot>();
        }

        public async Task<Guid> CreateLot(Lot lot)
        {
            if (lot is null) throw new ArgumentNullException(nameof(lot));

            var lotId = lot.Id == Guid.Empty ? Guid.NewGuid() : lot.Id;

            var entity = new LotEntity
            {
                Id = lotId,
                Name = lot.Name,
                Description = lot.Description,
                Price = lot.Price,
                Compensation = lot.Compensation,
                StockCount = lot.StockCount,
                DiscountedPrice = lot.DiscountedPrice,
                Type = lot.Type,
                Stage = LotStage.PendingApproval,
                DeclineReason = lot.DeclineReason,
                Category = new CategoryEntity
                {
                    Id = lot.Category.Id,
                    Name = lot.Category.Name,
                    Description = lot.Category.Description,
                    ParentCategoryId = lot.Category.ParentCategory?.Id
                },
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
                TicketsSold = (lot is DrawLot dl2) ? dl2.TicketsSold : null,
                IsDeleted = lot.IsDeleted,
                IsDrawn = (lot is DrawLot dl3) && dl3.IsDrawn,
                Characteristics = lot.Characteristics.Select(c => new CharacteristicEntity
                {
                    Id = Guid.NewGuid(),
                    Key = c.Key,
                    Value = c.Value,
                    LotId = lotId
                }).ToList(),
                Images = lot.Images.Select(i => new ImageEntity
                {
                    Id = i.Id,
                    Url = i.Url,
                    Data = i.Data?.ToArray(),
                    LotId = lotId
                }).ToList()
            };

            _db.Lots.Add(entity);

            if (entity.Seller.Id != Guid.Empty)
                _db.Entry(entity.Seller).State = EntityState.Unchanged;

            if (entity.Category.Id != Guid.Empty)
                _db.Entry(entity.Category).State = EntityState.Unchanged;

            await _db.SaveChangesAsync();

            return entity.Id;
        }

        public async Task UpdateLot(Guid id, Lot lot)
        {
            if (lot is null) throw new ArgumentNullException(nameof(lot));

            var existing = await _db.Lots
                .Include(l => l.Seller)
                .Include(l => l.Characteristics)
                .Include(l => l.Images)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (existing is null || existing.IsDeleted)
                throw new KeyNotFoundException($"Lot with id '{id}' not found.");

            existing.Name = lot.Name;
            existing.Description = lot.Description;
            existing.Price = lot.Price;
            existing.Compensation = lot.Compensation;
            existing.StockCount = lot.StockCount;
            existing.DiscountedPrice = lot.DiscountedPrice;
            existing.Type = lot.Type;
            existing.Stage = lot.Stage;
            existing.DeclineReason = lot.DeclineReason;
            existing.IsActive = lot.IsActive;
            existing.IsCompensationPaid = lot.IsCompensationPaid;
            existing.EndOfAuction = (lot is AuctionLot a) ? a.EndOfAuction : null;
            existing.AuctionStepPercent = (lot is AuctionLot a2) ? a2.AuctionStepPercent : null;
            existing.TicketPrice = (lot is DrawLot dl) ? dl.TicketPrice : existing.TicketPrice;
            existing.TicketsSold = (lot is DrawLot dl2) ? dl2.TicketsSold : existing.TicketsSold;
            existing.IsDrawn = (lot is DrawLot dlu) ? dlu.IsDrawn : existing.IsDrawn;
            existing.IsDeleted = lot.IsDeleted;

            if (lot.Seller is not null)
            {
                var seller = existing.Seller ?? throw new ArgumentException("The seller for this Lot doesn't exists! Please create a seller first.");

                seller.Name = lot.Seller.Name;
                seller.Description = lot.Seller.Description;
                seller.Email = lot.Seller.Email;
                seller.PhoneNumber = lot.Seller.PhoneNumber;
                seller.AvatarImageUrl = lot.Seller.AvatarImageUrl ?? string.Empty;
                seller.UserId = lot.Seller.UserId;
                seller.IsDeleted = seller.IsDeleted;

                if (existing.Seller == null)
                {
                    existing.Seller = seller;
                }
            }

            // Replace characteristics
            foreach (var c in existing.Characteristics.ToList())
                _db.Entry(c).State = EntityState.Deleted;

            existing.Characteristics.Clear();

            foreach (var c in lot.Characteristics)
            {
                existing.Characteristics.Add(new CharacteristicEntity
                {
                    Id = Guid.NewGuid(),
                    Key = c.Key,
                    Value = c.Value,
                    LotId = existing.Id
                });
            }

            // Replace images
            foreach (var image in existing.Images.ToList())
                _db.Entry(image).State = EntityState.Deleted;

            existing.Images.Clear();

            foreach (var image in lot.Images)
            {
                existing.Images.Add(new ImageEntity
                {
                    Id = image.Id,
                    Url = image.Url,
                    Data = image.Data?.ToArray(),
                    LotId = existing.Id
                });
            }

            _db.Lots.Update(existing);
            await _db.SaveChangesAsync();
        }

        private static Lot CreateFromEntity(LotEntity entity)
        {
            if (entity is null) throw new ArgumentNullException(nameof(entity));

            var sellerEntity = entity.Seller ?? throw new InvalidOperationException("Lot entity must have a Seller.");
            var categoryEntity = entity.Category ?? throw new InvalidOperationException("Lot entity must have a Category.");
            var category = new Category(categoryEntity.Id, categoryEntity.Name, categoryEntity.Description);

            var characteristics = entity.Characteristics
                .Select(c => new Characteristic(c.Key, c.Value))
                .ToArray();

            return entity.Type switch
            {
                LotType.Simple => new Lot(
                    entity.Id,
                    entity.Name,
                    entity.Description,
                    entity.Price,
                    entity.Compensation,
                    entity.StockCount,
                    entity.DiscountedPrice,
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
                    category,
                    entity.DeclineReason,
                    entity.IsDeleted,
                    characteristics),

                LotType.Auction => new AuctionLot(
                    entity.Id,
                    entity.Name,
                    entity.Description,
                    entity.Price,
                    entity.Compensation,
                    entity.StockCount,
                    entity.DiscountedPrice,
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
                    entity.AuctionStepPercent ?? throw new ArgumentNullException(nameof(entity.AuctionStepPercent)),
                    category,
                    entity.DeclineReason,
                    bidHistory: entity.BidHistory
                        .Select(b => new Bid(b.Id, b.AuctionId, b.BidderId, b.Amount, b.PlacedAt))
                        .ToList(),
                    isDeleted: entity.IsDeleted,
                    characteristics: characteristics),

                LotType.Draw => new DrawLot(
                    entity.Id,
                    entity.Name,
                    entity.Description,
                    entity.Price,
                    entity.Compensation,
                    entity.StockCount,
                    entity.DiscountedPrice,
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
                    entity.TicketPrice ?? throw new ArgumentNullException(nameof(entity.TicketPrice)),
                    ticketsSold: entity.TicketsSold ?? 0,
                    category,
                    entity.DeclineReason,
                    tickets: null,
                    isDrawn: entity.IsDrawn,
                    isDeleted: entity.IsDeleted,
                    characteristics: characteristics),

                _ => throw new InvalidOperationException($"Unsupported LotType: {entity.Type}")
            };
        }
    }
}
