using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Auth.Commands;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Auth;

public class LoginHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IRefreshTokenConfig _config = Substitute.For<IRefreshTokenConfig>();

    private readonly User _user = User.Create("testuser", "test@example.com", "hashed_password");

    private LoginHandler CreateHandler() =>
        new(_users, _hasher, _jwt, _refreshTokens, _uow, _config);

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsAuthResult()
    {
        _users.GetByEmailAsync("test@example.com").Returns(_user);
        _hasher.Verify("password123", "hashed_password").Returns(true);
        _hasher.Hash(Arg.Any<string>()).Returns("hashed_refresh");
        _jwt.GenerateAccessToken(_user).Returns("access_token");
        _jwt.GenerateRefreshToken().Returns("raw_refresh");
        _config.RefreshTokenDays.Returns(7);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new LoginCommand("test@example.com", "password123"));

        result.AccessToken.Should().Be("access_token");
        result.Username.Should().Be("testuser");
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _users.GetByEmailAsync("notfound@example.com").Returns((User?)null);

        var handler = CreateHandler();
        var act = () => handler.HandleAsync(new LoginCommand("notfound@example.com", "password123"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*credentials*");
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        _users.GetByEmailAsync("test@example.com").Returns(_user);
        _hasher.Verify("wrongpassword", "hashed_password").Returns(false);

        var handler = CreateHandler();
        var act = () => handler.HandleAsync(new LoginCommand("test@example.com", "wrongpassword"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*credentials*");
    }
}
