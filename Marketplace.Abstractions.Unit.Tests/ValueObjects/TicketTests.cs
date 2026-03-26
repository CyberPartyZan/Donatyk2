namespace Marketplace.Abstractions.Unit.Tests.ValueObjects
{
    public sealed class TicketTests
    {
        [Fact]
        public void Create_SetsDefaults()
        {
            var userId = Guid.NewGuid();
            var lotId = Guid.NewGuid();

            var before = DateTime.UtcNow;
            var ticket = Ticket.Create(userId, lotId);
            var after = DateTime.UtcNow;

            Assert.Equal(userId, ticket.UserId);
            Assert.Equal(lotId, ticket.LotId);
            Assert.False(ticket.IsWinning);
            Assert.False(ticket.IsPayed);
            Assert.InRange(ticket.CreatedAt, before, after);
        }

        [Fact]
        public void MarkAsPayed_SetsIsPayedTrue()
        {
            var ticket = Ticket.Create(Guid.NewGuid(), Guid.NewGuid());

            var payed = ticket.MarkAsPayed();

            Assert.True(payed.IsPayed);
            Assert.False(ticket.IsPayed);
        }
    }
}