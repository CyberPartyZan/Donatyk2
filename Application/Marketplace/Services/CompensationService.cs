using System.IO;
using System.Security.Claims;
using Marketplace.BlobStorage;
using Marketplace.Repository;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Marketplace
{
    public class CompensationService : ICompensationService
    {
        private const string ApprovementDocumentFilePath = "compensations/approvals";

        private readonly ICompensationRepository _compensationRepository;
        private readonly ClaimsPrincipal _user;
        private readonly IBlobStorageService _blobStorageService;

        public CompensationService(
            ICompensationRepository compensationRepository,
            ClaimsPrincipal user,
            IBlobStorageService blobStorageService)
        {
            _compensationRepository = compensationRepository;
            _user = user;
            _blobStorageService = blobStorageService;
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
            var compensation = await _compensationRepository.GetReadModel(id);
            return compensation is null ? null : Map(compensation);
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

        public async Task<string> GetApprovementDocumentUrl(Guid compensationId)
        {
            var compensation = await _compensationRepository.GetReadModel(compensationId)
                ?? throw new KeyNotFoundException($"Compensation '{compensationId}' not found.");

            if (compensation.ApprovementDocument is null)
                throw new FileNotFoundException($"Approval document is not attached for compensation '{compensationId}'.");

            var currentUserId = GetCurrentUserIdOrThrow();
            var isAdmin = _user.IsInRole("Admin");
            var isOwner = compensation.SellerUserId == currentUserId;

            if (!isAdmin && !isOwner)
                throw new UnauthorizedAccessException("Access denied to the approval document.");

            return await GetPresignedReadUrlAsync<Compensation>(compensationId, compensation.ApprovementDocument);
        }

        private async Task<string> GetPresignedReadUrlAsync<TResource>(Guid resourceId, BlobDto blob)
        {
            _ = resourceId; // keeps API generic for other resources
            _ = typeof(TResource);

            if (string.IsNullOrWhiteSpace(blob.Key) || string.IsNullOrWhiteSpace(blob.FilePath))
                throw new InvalidOperationException("Blob key and file path are required.");

            return await _blobStorageService.GetPresignedGetUrlAsync(blob.Key, blob.FilePath);
        }

        private Guid GetCurrentUserIdOrThrow()
        {
            var userId = _user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                         ?? _user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                throw new InvalidOperationException("User id is not available in the current principal.");

            return Guid.Parse(userId);
        }

        private static CompensationDto Map(CompensationReadModel x) => new()
        {
            Id = x.Id,
            OrderId = x.OrderId,
            LotId = x.LotId,
            Amount = x.Amount,
            Status = x.Status,
            SellerId = x.SellerId,
            SellerName = x.SellerName,
            ApprovementDocument = x.ApprovementDocument,

            SoldPrice = x.SoldPrice,
            SoldDate = x.SoldDate,
            BuyerName = x.BuyerName,
            LotName = x.LotName,
            LotImage = x.LotImage
        };

        public async Task<int> Process(
            IReadOnlyCollection<Guid> ids,
            Stream approvementDocumentStream,
            string approvementFileName)
        {
            if (ids is null || ids.Count == 0)
                throw new ArgumentException("At least one compensation id is required.", nameof(ids));

            if (approvementDocumentStream is null || !approvementDocumentStream.CanRead)
                throw new ArgumentException("Approval document stream is invalid.", nameof(approvementDocumentStream));

            if (string.IsNullOrWhiteSpace(approvementFileName))
                throw new ArgumentException("Approval document file name is required.", nameof(approvementFileName));

            var distinctIds = ids
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (distinctIds.Count == 0)
                throw new ArgumentException("At least one valid compensation id is required.", nameof(ids));

            var safeFileName = Path.GetFileName(approvementFileName.Trim());

            var key = await _blobStorageService.UploadAsync(approvementDocumentStream, ApprovementDocumentFilePath);

            try
            {
                var blob = new Blob(Guid.NewGuid(), ApprovementDocumentFilePath, key, safeFileName);
                return await _compensationRepository.Process(distinctIds, blob);
            }
            catch
            {
                await _blobStorageService.DeleteAsync(key, ApprovementDocumentFilePath);
                throw;
            }
        }
    }
}