using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Auth.Commands;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Auth;

public class RegisterHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IRefreshTokenConfig _config = Substitute.For<IRefreshTokenConfig>();

    private RegisterHandler CreateHandler() =>
        new(_users, _hasher, _jwt, _refreshTokens, _uow, _config);

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsAuthResult()
    {
        _users.ExistsByEmailAsync("test@example.com").Returns(false);
        _users.ExistsByUsernameAsync("testuser").Returns(false);
        _hasher.Hash("password123").Returns("hashed_password");
        _hasher.Hash(Arg.Is<string>(s => s != "password123")).Returns("hashed_refresh");
        _jwt.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        _jwt.GenerateRefreshToken().Returns("raw_refresh_token");
        _config.RefreshTokenDays.Returns(7);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new RegisterCommand("testuser", "test@example.com", "password123"));

        result.AccessToken.Should().Be("access_token");
        result.Username.Should().Be("testuser");
        await _users.Received(1).AddAsync(Arg.Any<User>());
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        _users.ExistsByEmailAsync("test@example.com").Returns(true);

        var handler = CreateHandler();
        var act = () => handler.HandleAsync(new RegisterCommand("testuser", "test@example.com", "password123"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Email*");
    }

    [Fact]
    public async Task HandleAsync_DuplicateUsername_ThrowsInvalidOperationException()
    {
        _users.ExistsByEmailAsync("test@example.com").Returns(false);
        _users.ExistsByUsernameAsync("testuser").Returns(true);

        var handler = CreateHandler();
        var act = () => handler.HandleAsync(new RegisterCommand("testuser", "test@example.com", "password123"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Username*");
    }
}
