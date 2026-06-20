using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TwitterClone.Application.Interfaces;
using TwitterClone.Infrastructure.Auth;
using TwitterClone.Infrastructure.Hubs;
using TwitterClone.Infrastructure.Persistence;
using TwitterClone.Infrastructure.Persistence.Repositories;
using TwitterClone.Infrastructure.Storage;

namespace TwitterClone.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITweetRepository, TweetRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IRetweetRepository, RetweetRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var workFactor = int.Parse(configuration["Bcrypt:WorkFactor"] ?? "11", System.Globalization.CultureInfo.InvariantCulture);
        services.AddSingleton<IPasswordHasher>(new BcryptPasswordHasher(workFactor));
        services.AddSingleton<ITokenHasher, Sha256TokenHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IRefreshTokenConfig, RefreshTokenConfig>();

        var uploadsPath = configuration["Storage:UploadsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        services.AddSingleton<IFileStorageService>(new LocalFileStorageService(uploadsPath));

        services.AddSignalR();
        services.AddScoped<ITimelineNotifier, SignalRTimelineNotifier>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                var key = configuration["Jwt:SigningKey"]
                    ?? throw new InvalidOperationException("Jwt:SigningKey missing");

                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                };
            });

        services.AddAuthorization();

        return services;
    }
}
