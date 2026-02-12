using Donatyk2.Server.Enums;
using Donatyk2.Server.ValueObjects;
using Marketplace.Repository.MSSql.Entities;

namespace Donatyk2.Server.Data
{
    internal class LotEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Money Price { get; set; }
        public Money Compensation { get; set; }
        public int StockCount { get; set; }
        public double Discount { get; set; }
        public LotType Type { get; set; }
        public LotStage Stage { get; set; }
        public string? DeclineReason { get; set; }
        public virtual SellerEntity Seller { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompensationPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public CategoryEntity Category { get; set; }
        public DateTime? EndOfAuction { get; set; }
        public int? AuctionStepPercent { get; set; }
        public Money? TicketPrice { get; set; }
        public bool IsDeleted { get; set; }
    }
}
