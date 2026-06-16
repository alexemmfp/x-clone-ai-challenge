using Microsoft.Extensions.Configuration;
using TwitterClone.Application.Interfaces;

namespace TwitterClone.Infrastructure.Auth;

internal sealed class RefreshTokenConfig : IRefreshTokenConfig
{
    public int RefreshTokenDays { get; }

    public RefreshTokenConfig(IConfiguration config)
    {
        RefreshTokenDays = int.Parse(config["Jwt:RefreshTokenDays"] ?? "7", System.Globalization.CultureInfo.InvariantCulture);
    }
}
