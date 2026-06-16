using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Auth.Commands;

public sealed record LogoutCommand(Guid UserId);

public sealed class LogoutHandler(IRefreshTokenRepository refreshTokens, IUnitOfWork uow)
{
    public async Task HandleAsync(LogoutCommand cmd, CancellationToken ct = default)
    {
        await refreshTokens.RevokeAllForUserAsync(cmd.UserId, ct);
        await uow.SaveChangesAsync(ct);
    }
}
