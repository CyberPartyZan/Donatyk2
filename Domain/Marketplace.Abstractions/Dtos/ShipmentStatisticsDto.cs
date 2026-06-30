namespace Marketplace
{
    public sealed class ShipmentStatisticsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int InTransit { get; set; }
        public int Delivered { get; set; }
    }
}