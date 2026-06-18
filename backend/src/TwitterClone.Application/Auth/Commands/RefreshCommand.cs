using TwitterClone.Application.Auth.Dtos;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Auth.Commands;

public sealed record RefreshCommand(string RefreshToken);

public sealed class RefreshHandler(
    IRefreshTokenRepository refreshTokens,
    IUserRepository users,
    ITokenHasher tokenHasher,
    IJwtService jwt,
    IUnitOfWork uow,
    IRefreshTokenConfig config)
{
    public async Task<AuthResult> HandleAsync(RefreshCommand cmd, CancellationToken ct = default)
    {
        var tokenHash = tokenHasher.Hash(cmd.RefreshToken);
        var stored = await refreshTokens.GetByTokenHashAsync(tokenHash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!stored.IsValid)
        {
            throw new UnauthorizedAccessException("Refresh token expired or revoked.");
        }

        stored.Revoke();

        var user = await users.GetByIdAsync(stored.UserId, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        var newRaw = jwt.GenerateRefreshToken();
        var newHash = tokenHasher.Hash(newRaw);
        var newToken = RefreshToken.Create(user.Id, newHash, DateTime.UtcNow.AddDays(config.RefreshTokenDays));
        await refreshTokens.AddAsync(newToken, ct);

        await uow.SaveChangesAsync(ct);

        return new AuthResult(jwt.GenerateAccessToken(user), newRaw, user.Id, user.Username);
    }
}
