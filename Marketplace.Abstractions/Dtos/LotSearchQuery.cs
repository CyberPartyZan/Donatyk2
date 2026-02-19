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
        public bool? GetDeleted { get; set; }
    }
}
