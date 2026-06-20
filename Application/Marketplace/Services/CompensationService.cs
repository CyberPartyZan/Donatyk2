using Marketplace.Repository;

namespace Marketplace
{
    public class CompensationService : ICompensationService
    {
        private readonly ICompensationRepository _compensationRepository;

        public CompensationService(ICompensationRepository compensationRepository)
        {
            _compensationRepository = compensationRepository;
        }

        public async Task<Guid> Create(Guid orderId, Guid lotId, Money amount)
        {
            var compensation = Compensation.Create(orderId, lotId, amount);
            return await _compensationRepository.Create(compensation);
        }

        public async Task<Guid> CreateIfNotExists(Guid orderId, Guid lotId, Money amount)
        {
            if (await _compensationRepository.Exists(orderId, lotId))
                return Guid.Empty;

            return await Create(orderId, lotId, amount);
        }

        public async Task<CompensationDto?> Get(Guid id)
        {
            var compensation = await _compensationRepository.Get(id);
            if (compensation is null) return null;

            return new CompensationDto
            {
                Id = compensation.Id,
                OrderId = compensation.OrderId,
                LotId = compensation.LotId,
                Amount = compensation.Amount,
                Status = compensation.Status
            };
        }

        public async Task<IReadOnlyCollection<CompensationDto>> GetBySellerId(Guid sellerId, CompensationStatus? status = null)
        {
            var data = await _compensationRepository.GetBySellerId(sellerId, status);
            return data.Select(Map).ToList();
        }

        public async Task<CompensationGroupedPageDto> GetAll(int page, int pageSize, CompensationStatus? status = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var (items, totalGroups) = await _compensationRepository.GetAll(page, pageSize, status);

            var grouped = items
                .GroupBy(x => new { x.SellerId, x.SellerName })
                .OrderBy(g => g.Key.SellerName)
                .Select(g => new CompensationSellerGroupDto
                {
                    SellerId = g.Key.SellerId,
                    SellerName = g.Key.SellerName,
                    Compensations = g.Select(Map).ToList()
                })
                .ToList();

            return new CompensationGroupedPageDto
            {
                Page = page,
                PageSize = pageSize,
                TotalGroups = totalGroups,
                Items = grouped
            };
        }

        public async Task Update(IReadOnlyCollection<Guid> ids, CompensationStatus status)
        {
            if (ids.Count == 0)
                return;

            var compensations = new List<Compensation>();

            foreach (var id in ids.Distinct())
            {
                var compensation = await _compensationRepository.Get(id)
                    ?? throw new KeyNotFoundException($"Compensation '{id}' not found.");

                compensation.SetStatus(status);
                compensations.Add(compensation);
            }

            await _compensationRepository.Update(compensations);
        }

        public async Task<int> RequestCompensation(Guid sellerId)
        {
            var pending = await _compensationRepository.GetBySellerId(sellerId, CompensationStatus.Pending);
            if (pending.Count == 0)
                return 0;

            var compensations = pending
                .Select(x => new Compensation(x.Id, x.OrderId, x.LotId, x.Amount, x.Status))
                .ToList();

            foreach (var compensation in compensations)
                compensation.MarkRequested();

            await _compensationRepository.Update(compensations);
            return compensations.Count;
        }

        private static CompensationDto Map(CompensationReadModel x) => new()
        {
            Id = x.Id,
            OrderId = x.OrderId,
            LotId = x.LotId,
            Amount = x.Amount,
            Status = x.Status,
            SellerId = x.SellerId,
            SellerName = x.SellerName
        };
    }
}