namespace Marketplace.BlobStorage
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(Stream file, string filePath);
        Task DeleteAsync(string key, string filePath);
        Task<Stream> DownloadAsync(string key, string filePath);
    }
}
