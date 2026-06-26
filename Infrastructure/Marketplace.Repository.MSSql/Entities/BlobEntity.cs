namespace Marketplace.Repository.MSSql
{
    internal class BlobEntity
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
