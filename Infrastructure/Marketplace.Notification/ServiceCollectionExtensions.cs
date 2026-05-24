using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SendGrid;

namespace Marketplace.Notification
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions<SendGridOptions>()
                .Bind(configuration.GetSection(SendGridOptions.SectionName));

            services.AddSingleton<ISendGridClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<SendGridOptions>>().Value;
                return new SendGridClient(options.ApiKey);
            });

            services.AddScoped<INotificationService, SendGridNotificationService>();

            return services;
        }
    }
}
