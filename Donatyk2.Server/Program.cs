using Donatyk2.Server.Controllers;
using Donatyk2.Server.Data;
using Donatyk2.Server.Services;
using Donatyk2.Server.Services.Interfaces;
using Donatyk2.Server.Settings;
using Donatyk2.Server.Repositories;
using Donatyk2.Server.Repositories.Interfaces;
using Donatyk2.Server.Services.Payments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Security.Claims;

namespace Donatyk2.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // read allowed origins from configuration (Spa:AllowedOrigins)
            var allowedOrigins = builder.Configuration
                .GetSection("Spa:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("spa", policy =>
                {
                    // if configured as ["*"] then allow any origin (cannot use AllowCredentials with AllowAnyOrigin)
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
                        // fallback: conservative default (no origins) — change as appropriate
                        policy.DisallowCredentials()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                });
            });

            // Add services to the container.
            builder.Services.AddDbContext<DonatykDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services
                .AddIdentityCore<ApplicationUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.User.RequireUniqueEmail = true;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<DonatykDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            // TODO: Use options pattern to bind JWT settings
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

            // core helpers
            builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

            // repositories
            builder.Services.AddScoped<ILotsRepository, LotsRepository>();
            builder.Services.AddScoped<ISellersRepository, SellersRepository>();
            builder.Services.AddScoped<IUsersRepository, UsersRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

            // services
            builder.Services.AddScoped<ILotsService, LotsService>();
            builder.Services.AddScoped<ISellersService, SellersService>();
            builder.Services.AddScoped<IUsersService, UsersService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrdersService, OrdersService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IPaymentGateway, FakePaymentGateway>();

            // Ensure HttpContext is available to services that depend on ClaimsPrincipal
            builder.Services.AddHttpContextAccessor();
            // Provide ClaimsPrincipal via DI (scoped) so services can accept it in constructors
            builder.Services.AddScoped<ClaimsPrincipal>(sp =>
                sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new ClaimsPrincipal());

            var app = builder.Build();

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


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
