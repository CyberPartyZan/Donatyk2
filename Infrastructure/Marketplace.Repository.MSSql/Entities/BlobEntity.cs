namespace Marketplace.Repository.MSSql.Entities
{
    internal class BlobEntity
    {
        public Guid Id {  get; set; }
        public string FilePath { get; set; }
        public string Key { get; set; }
    }
}
