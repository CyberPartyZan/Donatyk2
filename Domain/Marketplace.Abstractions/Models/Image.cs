namespace Marketplace
{
    public class Image
    {
        public Guid Id { get; }
        public string? Url { get; }
        public byte[]? Data { get; }

        public Image(Guid id, string? url, byte[]? data)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Image id cannot be empty.", nameof(id));

            var normalizedUrl = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
            var normalizedData = data is { Length: > 0 } ? data.ToArray() : null;

            if (normalizedUrl is null && normalizedData is null)
                throw new ArgumentException("Either Url or Data must be provided.");

            Id = id;
            Url = normalizedUrl;
            Data = normalizedData;
        }
    }
}