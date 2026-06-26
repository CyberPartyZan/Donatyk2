using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Marketplace.BlobStorage
{
    internal sealed class MinIOBlobStorageService : IBlobStorageService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinIOOptions _options;

        public MinIOBlobStorageService(
            IMinioClient minioClient,
            IOptions<MinIOOptions> options)
        {
            _minioClient = minioClient;
            _options = options.Value;
        }

        public async Task<string> UploadAsync(Stream file, string filePath)
        {
            ArgumentNullException.ThrowIfNull(file);
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            await EnsureBucketExistsAsync();

            var key = Guid.NewGuid().ToString("N");
            var objectName = BuildObjectName(filePath, key);

            Stream uploadStream = file;
            if (!file.CanSeek)
            {
                var buffered = new MemoryStream();
                await file.CopyToAsync(buffered);
                buffered.Position = 0;
                uploadStream = buffered;
            }

            if (uploadStream.CanSeek)
            {
                uploadStream.Position = 0;
            }

            var putArgs = new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectName)
                .WithStreamData(uploadStream)
                .WithObjectSize(uploadStream.Length)
                .WithContentType("application/octet-stream");

            await _minioClient.PutObjectAsync(putArgs);
            return key;
        }

        public async Task DeleteAsync(string key, string filePath)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key is required.", nameof(key));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            var objectName = BuildObjectName(filePath, key);

            var removeArgs = new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeArgs);
        }

        public async Task<Stream> DownloadAsync(string key, string filePath)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key is required.", nameof(key));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            var objectName = BuildObjectName(filePath, key);
            var output = new MemoryStream();

            var getArgs = new GetObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(output));

            await _minioClient.GetObjectAsync(getArgs);

            output.Position = 0;
            return output;
        }

        private async Task EnsureBucketExistsAsync()
        {
            var existsArgs = new BucketExistsArgs().WithBucket(_options.BucketName);
            var exists = await _minioClient.BucketExistsAsync(existsArgs);

            if (exists)
                return;

            var makeBucketArgs = new MakeBucketArgs().WithBucket(_options.BucketName);
            await _minioClient.MakeBucketAsync(makeBucketArgs);
        }

        private static string BuildObjectName(string filePath, string key)
        {
            var normalizedPath = filePath.Trim().Trim('/').Replace("\\", "/", StringComparison.Ordinal);
            return $"{normalizedPath}/{key}";
        }
    }
}
