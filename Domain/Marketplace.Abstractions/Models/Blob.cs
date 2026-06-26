namespace Marketplace
{
    public class Blob
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; }
        public string Key { get; set; }
        public string FileName { get; set; }

        public Blob(Guid id, string filePath, string key, string fileName)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or whitespace.", nameof(fileName));

            Id = id;
            FilePath = filePath;
            Key = key;
            FileName = fileName;
        }
    }
}
