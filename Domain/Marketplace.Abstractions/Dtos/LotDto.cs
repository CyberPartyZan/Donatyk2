namespace Marketplace
{
    public class LotDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Money Price { get; set; }
        public Money Compensation { get; set; }
        public int StockCount { get; set; }
        public Money? DiscountedPrice { get; set; }
        public double Discount { get; set; }
        public LotType Type { get; set; }
        public LotStage Stage { get; set; }
        public string? DeclineReason { get; set; }
        public SellerDto Seller { get; set; }
        public required CategoryDto Category { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompensationPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndOfAuction { get; set; }
        public int? AuctionStepPercent { get; set; }
        public Money? TicketPrice { get; set; }
    }
}
