using FluentValidation;
using TwitterClone.Application.Auth.Dtos;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    ITokenHasher tokenHasher,
    IJwtService jwt,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork uow,
    IRefreshTokenConfig config)
{
    public async Task<AuthResult> HandleAsync(LoginCommand cmd, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(cmd.Email, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!hasher.Verify(cmd.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var rawToken = jwt.GenerateRefreshToken();
        var tokenHash = tokenHasher.Hash(rawToken);
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, DateTime.UtcNow.AddDays(config.RefreshTokenDays));
        await refreshTokens.AddAsync(refreshToken, ct);

        await uow.SaveChangesAsync(ct);

        return new AuthResult(jwt.GenerateAccessToken(user), rawToken, user.Id, user.Username);
    }
}
