using Donatyk2.Server.Models;
using Donatyk2.Server.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;

namespace Donatyk2.Server.Data
{
    public class LotEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double Compensation { get; set; }
        public LotType Type { get; set; }
        public SellerEntity Seller { get; set; }
        public bool IsActive { get; set; }
        public bool IsCompensationPaid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EndOfAuction { get; set; }
        public int? AuctionStepPercent { get; set; }
        public double? TicketPrice { get; set; }
    }
}
