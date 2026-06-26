namespace Marketplace.BlobStorage
{
    public sealed class MinIOOptions
    {
        public const string SectionName = "MinIO";

        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
    }
}