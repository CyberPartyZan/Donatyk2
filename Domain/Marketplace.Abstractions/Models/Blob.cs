namespace Marketplace
{
    public class Blob
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; }
        public string Key { get; set; }

        public Blob(Guid id, string filePath, string key)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }

            Id = id;
            FilePath = filePath;
            Key = key;
        }
    }
}
