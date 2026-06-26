namespace Marketplace.Repository.MSSql
{
    internal class BlobEntity
    {
        public Guid Id {  get; set; }
        public string FilePath { get; set; }
        public string Key { get; set; }
    }
}
