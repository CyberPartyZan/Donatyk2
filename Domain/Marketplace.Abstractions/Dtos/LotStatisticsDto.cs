namespace Marketplace
{
    public sealed class LotStatisticsDto
    {
        public int Total { get; set; }
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Active { get; set; }
    }
}