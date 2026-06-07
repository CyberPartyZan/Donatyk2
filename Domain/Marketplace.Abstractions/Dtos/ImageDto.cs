namespace Marketplace
{
    public class ImageDto
    {
        public Guid Id { get; set; }
        public string? Url { get; set; }
        public byte[]? Data { get; set; }
    }
}