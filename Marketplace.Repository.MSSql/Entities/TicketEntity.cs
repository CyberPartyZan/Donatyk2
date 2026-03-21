namespace Marketplace.Repository.MSSql
{
    internal class TicketEntity
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public virtual ApplicationUser User { get; set; } = null!;

        public Guid LotId { get; set; }
        public virtual LotEntity Lot { get; set; } = null!;

        public bool IsWinning { get; set; }
    }
}