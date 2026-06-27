using Microsoft.EntityFrameworkCore;

namespace Marketplace.Repository.MSSql
{
    internal sealed class CompensationRepository : ICompensationRepository
    {
        private readonly MarketplaceDbContext _db;

        public CompensationRepository(MarketplaceDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> Create(Compensation compensation)
        {
            var entity = new CompensationEntity
            {
                Id = compensation.Id,
                OrderId = compensation.OrderId,
                LotId = compensation.LotId,
                Amount = compensation.Amount,
                Status = compensation.Status
            };

            _db.Compensations.Add(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<Compensation?> Get(Guid id)
        {
            var e = await _db.Compensations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return e is null ? null : new Compensation(e.Id, e.OrderId, e.LotId, e.Amount, e.Status);
        }

        public async Task<CompensationReadModel?> GetReadModel(Guid id)
        {
            return await _db.Compensations
                .AsNoTracking()
                .Include(c => c.Order).ThenInclude(o => o.Items)
                .Include(c => c.Lot).ThenInclude(l => l.Seller)
                .Include(c => c.Lot).ThenInclude(l => l.Images)
                .Include(c => c.ApprovementDocument)
                .Where(c => c.Id == id)
                .Select(c => new CompensationReadModel
                {
                    Id = c.Id,
                    OrderId = c.OrderId,
                    LotId = c.LotId,
                    Amount = c.Amount,
                    Status = c.Status,
                    SellerId = c.Lot.Seller.Id,
                    SellerUserId = c.Lot.Seller.UserId,
                    SellerName = c.Lot.Seller.Name,
                    ApprovementDocument = c.ApprovementDocument == null
                        ? null
                        : new BlobDto
                        {
                            Id = c.ApprovementDocument.Id,
                            Key = c.ApprovementDocument.Key,
                            FilePath = c.ApprovementDocument.FilePath,
                            FileName = c.ApprovementDocument.FileName
                        },

                    SoldPrice = c.Order.Items
                        .Where(i => i.LotId == c.LotId)
                        .Select(i => i.UnitPrice.Amount * i.Quantity)
                        .FirstOrDefault(),
                    SoldDate = c.Order.CreatedAt,
                    BuyerName = _db.ApplicationUsers
                        .Where(u => u.Id == c.Order.CustomerId)
                        .Select(u => u.UserName ?? u.Email ?? string.Empty)
                        .FirstOrDefault() ?? string.Empty,
                    LotName = c.Order.Items
                        .Where(i => i.LotId == c.LotId)
                        .Select(i => i.NameSnapshot)
                        .FirstOrDefault() ?? c.Lot.Name,
                    LotImage = c.Lot.Images
                        .OrderBy(i => i.Id)
                        .Select(i => i.FilePath + "/" + i.Key)
                        .FirstOrDefault() ?? string.Empty
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<CompensationReadModel>> GetBySellerId(Guid sellerId, CompensationStatus? status = null)
        {
            var q = _db.Compensations
                .AsNoTracking()
                .Include(c => c.Order).ThenInclude(o => o.Items)
                .Include(c => c.Lot).ThenInclude(l => l.Seller)
                .Include(c => c.Lot).ThenInclude(l => l.Images)
                .Include(c => c.ApprovementDocument)
                .Where(c => c.Lot.Seller.Id == sellerId);

            if (status.HasValue)
                q = q.Where(c => c.Status == status.Value);

            return await q.Select(c => new CompensationReadModel
            {
                Id = c.Id,
                OrderId = c.OrderId,
                LotId = c.LotId,
                Amount = c.Amount,
                Status = c.Status,
                SellerId = c.Lot.Seller.Id,
                SellerUserId = c.Lot.Seller.UserId,
                SellerName = c.Lot.Seller.Name,
                ApprovementDocument = c.ApprovementDocument == null
                    ? null
                    : new BlobDto
                    {
                        Id = c.ApprovementDocument.Id,
                        Key = c.ApprovementDocument.Key,
                        FilePath = c.ApprovementDocument.FilePath,
                        FileName = c.ApprovementDocument.FileName
                    },

                SoldPrice = c.Order.Items
                    .Where(i => i.LotId == c.LotId)
                    .Select(i => i.UnitPrice.Amount * i.Quantity)
                    .FirstOrDefault(),
                SoldDate = c.Order.CreatedAt,
                BuyerName = _db.ApplicationUsers
                    .Where(u => u.Id == c.Order.CustomerId)
                    .Select(u => u.UserName ?? u.Email ?? string.Empty)
                    .FirstOrDefault() ?? string.Empty,
                LotName = c.Order.Items
                    .Where(i => i.LotId == c.LotId)
                    .Select(i => i.NameSnapshot)
                    .FirstOrDefault() ?? c.Lot.Name,
                LotImage = c.Lot.Images
                    .OrderBy(i => i.Id)
                    .Select(i => i.FilePath + "/" + i.Key)
                    .FirstOrDefault() ?? string.Empty
            }).ToListAsync();
        }

        public async Task<(IReadOnlyCollection<CompensationReadModel> Items, int TotalGroups)> GetAll(int page, int pageSize, CompensationStatus? status = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var baseQuery = _db.Compensations
                .AsNoTracking()
                .Include(c => c.Order).ThenInclude(o => o.Items)
                .Include(c => c.Lot).ThenInclude(l => l.Seller)
                .Include(c => c.Lot).ThenInclude(l => l.Images)
                .Include(c => c.ApprovementDocument)
                .AsQueryable();

            if (status.HasValue)
                baseQuery = baseQuery.Where(c => c.Status == status.Value);

            var groupsQuery = baseQuery
                .Select(c => new { SellerId = c.Lot.Seller.Id, SellerName = c.Lot.Seller.Name })
                .Distinct();

            var totalGroups = await groupsQuery.CountAsync();

            var pagedSellerIds = await groupsQuery
                .OrderBy(g => g.SellerName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => g.SellerId)
                .ToListAsync();

            if (pagedSellerIds.Count == 0)
                return (Array.Empty<CompensationReadModel>(), totalGroups);

            var items = await baseQuery
                .Where(c => pagedSellerIds.Contains(c.Lot.Seller.Id))
                .Select(c => new CompensationReadModel
                {
                    Id = c.Id,
                    OrderId = c.OrderId,
                    LotId = c.LotId,
                    Amount = c.Amount,
                    Status = c.Status,
                    SellerId = c.Lot.Seller.Id,
                    SellerUserId = c.Lot.Seller.UserId,
                    SellerName = c.Lot.Seller.Name,
                    ApprovementDocument = c.ApprovementDocument == null
                        ? null
                        : new BlobDto
                        {
                            Id = c.ApprovementDocument.Id,
                            Key = c.ApprovementDocument.Key,
                            FilePath = c.ApprovementDocument.FilePath,
                            FileName = c.ApprovementDocument.FileName
                        },

                    SoldPrice = c.Order.Items
                        .Where(i => i.LotId == c.LotId)
                        .Select(i => i.UnitPrice.Amount * i.Quantity)
                        .FirstOrDefault(),
                    SoldDate = c.Order.CreatedAt,
                    BuyerName = _db.ApplicationUsers
                        .Where(u => u.Id == c.Order.CustomerId)
                        .Select(u => u.UserName ?? u.Email ?? string.Empty)
                        .FirstOrDefault() ?? string.Empty,
                    LotName = c.Order.Items
                        .Where(i => i.LotId == c.LotId)
                        .Select(i => i.NameSnapshot)
                        .FirstOrDefault() ?? c.Lot.Name,
                    LotImage = c.Lot.Images
                        .OrderBy(i => i.Id)
                        .Select(i => i.FilePath + "/" + i.Key)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            return (items, totalGroups);
        }

        public async Task Update(IReadOnlyCollection<Compensation> compensations)
        {
            if (compensations.Count == 0)
                return;

            var requested = compensations
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            var ids = requested.Select(c => c.Id).ToList();

            var entities = await _db.Compensations
                .Where(x => ids.Contains(x.Id))
                .ToListAsync();

            if (entities.Count != ids.Count)
            {
                var missing = ids.Except(entities.Select(e => e.Id)).First();
                throw new KeyNotFoundException($"Compensation '{missing}' not found.");
            }

            var map = requested.ToDictionary(c => c.Id);

            foreach (var entity in entities)
            {
                var source = map[entity.Id];
                entity.Status = source.Status;
                entity.Amount = source.Amount;
            }

            await _db.SaveChangesAsync();
        }

        public Task<bool> Exists(Guid orderId, Guid lotId) =>
            _db.Compensations.AnyAsync(c => c.OrderId == orderId && c.LotId == lotId);

        public async Task<int> Process(IReadOnlyCollection<Guid> ids, Blob approvementDocument)
        {
            ArgumentNullException.ThrowIfNull(approvementDocument);

            if (ids.Count == 0)
                return 0;

            var requestedIds = ids
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (requestedIds.Count == 0)
                return 0;

            var entities = await _db.Compensations
                .Where(x => requestedIds.Contains(x.Id))
                .ToListAsync();

            if (entities.Count != requestedIds.Count)
            {
                var missing = requestedIds.Except(entities.Select(e => e.Id)).First();
                throw new KeyNotFoundException($"Compensation '{missing}' not found.");
            }

            var processable = entities
                .Where(x => x.Status == CompensationStatus.Pending || x.Status == CompensationStatus.Requested)
                .ToList();

            if (processable.Count == 0)
                throw new InvalidOperationException("No compensations are eligible for processing.");

            var blobEntity = new BlobEntity
            {
                Id = approvementDocument.Id,
                FilePath = approvementDocument.FilePath,
                Key = approvementDocument.Key,
                FileName = approvementDocument.FileName
            };

            _db.Blobs.Add(blobEntity);

            // TODO: Move this logic to a domain model or application service layer, and only update compesation here.
            foreach (var entity in processable)
            {
                entity.Status = CompensationStatus.Paid;
                entity.ApprovementDocumentId = blobEntity.Id;
            }

            await _db.SaveChangesAsync();
            return processable.Count;
        }
    }
}