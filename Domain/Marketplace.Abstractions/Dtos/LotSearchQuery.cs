namespace Marketplace
{
    public class LotSearchQuery
    {
        public string? SearchText { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public Guid? SellerId { get; set; }
        public LotType? Type { get; set; }
        public int? MinDiscount { get; set; }
        public int? MaxDiscount { get; set; }
        public Guid? CategoryId { get; set; }
        public bool? GetDeleted { get; set; }
        public bool? GetExhausted { get; set; }
        public bool? GetInactive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public bool IsCacheSupported()
        {
            return string.IsNullOrWhiteSpace(SearchText)
                && MinPrice is null
                && MaxPrice is null
                && MinDiscount is null
                && MaxDiscount is null
                && GetDeleted is null
                && GetExhausted is null
                && GetInactive is null;
        }

        public string ToCacheKey()
        {
            var sellerKey = SellerId?.ToString("N") ?? "any";
            var typeKey = Type?.ToString() ?? "any";
            var categoryKey = CategoryId?.ToString("N") ?? "any";
            var pageNumber = PageNumber > 0 ? PageNumber : 1;
            var pageSize = PageSize > 0 ? PageSize : 20;

            return $"lots:all:seller:{sellerKey}:type:{typeKey}:category:{categoryKey}:page:{pageNumber}:size:{pageSize}";
        }
    }
}
