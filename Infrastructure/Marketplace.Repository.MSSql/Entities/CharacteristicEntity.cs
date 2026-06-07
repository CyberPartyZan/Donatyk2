namespace Marketplace.Repository.MSSql
{
    internal class CharacteristicEntity
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public Guid LotId { get; set; }
        public virtual LotEntity Lot { get; set; }
    }
}