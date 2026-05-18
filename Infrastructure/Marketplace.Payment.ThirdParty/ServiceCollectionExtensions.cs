using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Stripe;

namespace Marketplace.Payment
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPaymentServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddOptions<PaymentGatewaySettings>()
                .Bind(configuration.GetSection(PaymentGatewaySettings.SectionName))
                .ValidateOnStart();

            services
                .AddOptions<StripeSettings>()
                .Bind(configuration.GetSection(StripeSettings.SectionName))
                .ValidateOnStart();

            var gatewayProvider = configuration
                .GetSection(PaymentGatewaySettings.SectionName)
                .GetValue<string>("Provider") ?? "Fake";

            if (string.Equals(gatewayProvider, "Stripe", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<IStripeClient>(sp =>
                {
                    var settings = sp.GetRequiredService<IOptions<StripeSettings>>().Value;
                    return new StripeClient(settings.SecretKey);
                });
                services.AddSingleton<IPaymentGateway, StripePaymentGateway>();
            }
            else
            {
                services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
            }

            return services;
        }
    }
}
