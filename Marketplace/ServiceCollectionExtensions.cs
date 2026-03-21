using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Marketplace
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMarketplaceServices(this IServiceCollection services)
        {
            services.AddScoped<ILotsService, LotsService>();
            services.AddScoped<ISellersService, SellersService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrdersService, OrdersService>();
            services.AddScoped<ICategoriesService, CategoryService>();
            services.AddScoped<ITicketsService, TicketsService>();
            services.AddScoped<IBidsService, BidsService>();

            services.AddSingleton<IPaymentGateway, FakePaymentGateway>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCreatedConsumer>();
                x.AddConsumer<PaymentProcessedConsumer>();
                x.AddConsumer<ShipmentServicePaymentProcessedConsumer>();
                x.AddConsumer<ShipmentCreatedConsumer>();

                //x.AddEntityFrameworkOutbox<AppDbContext>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    // TODO: Move RabbitMQ configuration to appsettings and use options pattern
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
