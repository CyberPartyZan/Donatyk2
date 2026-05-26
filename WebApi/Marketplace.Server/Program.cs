using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Marketplace.Repository.MSSql;
using Marketplace.Authentication.JWT;
using Marketplace.Notification;
using Marketplace.Cache;
using Marketplace.Server.Identity;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Hangfire;
using Marketplace.Payment;

namespace Marketplace.Server
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var openTelemetrySection = builder.Configuration.GetSection("OpenTelemetry");
            var serviceName = openTelemetrySection.GetValue<string>("ServiceName") ?? "Marketplace.Server";
            var otlpEndpoint = openTelemetrySection.GetValue<string>("Otlp:Endpoint") ?? "http://localhost:4317";
            var otlpEndpointUri = new Uri(otlpEndpoint);

            builder.Services
                .AddOptions<JwtSettings>()
                .Bind(builder.Configuration.GetSection("Jwt"))
                .Validate(settings => !string.IsNullOrWhiteSpace(settings.Key), "JWT key must be configured.")
                .ValidateOnStart();

            builder.Services
                .AddOptions<AdminUserOptions>()
                .Bind(builder.Configuration.GetSection("AdminUser"));

            var allowedOrigins = builder.Configuration
                .GetSection("Spa:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("spa", policy =>
                {
                    if (allowedOrigins.Length == 1 && allowedOrigins[0] == "*")
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else
                    {
                        policy.DisallowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            builder.Services.AddMarketplaceRepositoryServices(builder.Configuration.GetConnectionString("DefaultConnection"));
            builder.Services.AddAuthenticationServices();
            builder.Services.AddNotificationServices(builder.Configuration);
            builder.Services.AddCacheServices(builder.Configuration);
            builder.Services.AddMarketplaceServices(
                builder.Configuration["MassTransit:EndpointPrefix"]);
            builder.Services.AddPaymentServices(builder.Configuration);

            // Hangfire — using in-memory storage (swap to SQL Server in production via Hangfire.SqlServer)
            builder.Services.AddHangfire(config =>
                config.UseInMemoryStorage());
            builder.Services.AddHangfireServer();

            builder.Logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpointUri);
            });

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpointUri))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(exporter => exporter.Endpoint = otlpEndpointUri));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>>(sp =>
                new ConfigureNamedOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var jwt = sp.GetRequiredService<IOptions<JwtSettings>>().Value;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt.Key ?? string.Empty)
                        )
                    };
                }));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ClaimsPrincipal>(sp =>
                sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new ClaimsPrincipal());

            var app = builder.Build();

            //await app.SeedAdminUserAsync();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("spa");

            app.UseAuthentication();
            app.UseAuthorization();

            // Register recurring Hangfire job — every 5 minutes
            var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<CheckAuctionEndedJob>(
                "check-auction-ended",
                job => job.ExecuteAsync(CancellationToken.None),
                "*/5 * * * *");

            app.UseHangfireDashboard("/hangfire");

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
