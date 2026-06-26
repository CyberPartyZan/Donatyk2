using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace Marketplace.BlobStorage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlobStorageServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<MinIOOptions>()
                .Bind(configuration.GetSection(MinIOOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.Endpoint), "MinIO endpoint must be configured.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.AccessKey), "MinIO access key must be configured.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.SecretKey), "MinIO secret key must be configured.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.BucketName), "MinIO bucket name must be configured.")
                .ValidateOnStart();

            services.AddSingleton<IMinioClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MinIOOptions>>().Value;
                var clientBuilder = new MinioClient()
                    .WithEndpoint(options.Endpoint)
                    .WithCredentials(options.AccessKey, options.SecretKey);

                if (options.UseSsl)
                {
                    clientBuilder = clientBuilder.WithSSL();
                }

                return clientBuilder.Build();
            });

            services.AddScoped<IBlobStorageService, MinIOBlobStorageService>();

            return services;
        }
    }
}
