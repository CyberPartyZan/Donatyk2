namespace Marketplace
{
    public class BlobDto
    {
        public Guid Id { get; set; }
        public string? FilePath { get; set; }
        public string? Key { get; set; }
        public string? FileName { get; set; }
    }
}