using FluentValidation;
using TwitterClone.Application.Auth.Dtos;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Auth.Commands;

public sealed record RegisterCommand(string Username, string Email, string Password);

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);
    }
}

public sealed class RegisterHandler(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtService jwt,
    IRefreshTokenRepository refreshTokens,
    IUnitOfWork uow,
    IRefreshTokenConfig config)
{
    public async Task<AuthResult> HandleAsync(RegisterCommand cmd, CancellationToken ct = default)
    {
        if (await users.ExistsByEmailAsync(cmd.Email, ct))
        {
            throw new InvalidOperationException("Email already in use.");
        }

        if (await users.ExistsByUsernameAsync(cmd.Username, ct))
        {
            throw new InvalidOperationException("Username already taken.");
        }

        var user = User.Create(cmd.Username, cmd.Email, hasher.Hash(cmd.Password));
        await users.AddAsync(user, ct);

        var rawToken = jwt.GenerateRefreshToken();
        var tokenHash = hasher.Hash(rawToken);
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, DateTime.UtcNow.AddDays(config.RefreshTokenDays));
        await refreshTokens.AddAsync(refreshToken, ct);

        await uow.SaveChangesAsync(ct);

        return new AuthResult(jwt.GenerateAccessToken(user), rawToken, user.Id, user.Username);
    }
}
