using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Notification
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotificationServices(this IServiceCollection services)
        {
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
