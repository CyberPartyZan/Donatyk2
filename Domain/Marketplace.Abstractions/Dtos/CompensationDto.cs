namespace Marketplace
{
    public sealed class CompensationDto
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid LotId { get; set; }
        public Money Amount { get; set; } = null!;
        public CompensationStatus Status { get; set; }
        public Guid SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
    }

    public sealed class CompensationSellerGroupDto
    {
        public Guid SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public IReadOnlyCollection<CompensationDto> Compensations { get; set; } = Array.Empty<CompensationDto>();
    }

    public sealed class CompensationGroupedPageDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalGroups { get; set; }
        public IReadOnlyCollection<CompensationSellerGroupDto> Items { get; set; } = Array.Empty<CompensationSellerGroupDto>();
    }

    public sealed class CompensationReadModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid LotId { get; set; }
        public Money Amount { get; set; } = null!;
        public CompensationStatus Status { get; set; }
        public Guid SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
    }
}