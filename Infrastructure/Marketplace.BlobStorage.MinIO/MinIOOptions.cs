namespace Marketplace.BlobStorage
{
    public sealed class MinIOOptions
    {
        public const string SectionName = "MinIO";

        public string Endpoint { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
    }
}