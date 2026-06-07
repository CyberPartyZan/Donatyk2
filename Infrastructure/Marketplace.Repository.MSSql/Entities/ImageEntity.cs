namespace Marketplace.Repository.MSSql
{
    internal class ImageEntity
    {
        public Guid Id { get; set; }
        public string? Url { get; set; }
        public byte[]? Data { get; set; }

        public Guid LotId { get; set; }
        public virtual LotEntity Lot { get; set; }
    }
}