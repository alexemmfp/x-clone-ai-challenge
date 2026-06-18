using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Auth.Commands;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Auth;

public class RefreshHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITokenHasher _tokenHasher = Substitute.For<ITokenHasher>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IRefreshTokenConfig _config = Substitute.For<IRefreshTokenConfig>();

    private readonly User _user = User.Create("alice", "alice@example.com", "hash");

    private RefreshHandler CreateHandler() => new(_refreshTokens, _users, _tokenHasher, _jwt, _uow, _config);

    [Fact]
    public async Task HandleAsync_ValidToken_ReturnsNewAuthResult()
    {
        var stored = RefreshToken.Create(_user.Id, "hash", DateTime.UtcNow.AddDays(7));
        _tokenHasher.Hash("raw").Returns("hash");
        _refreshTokens.GetByTokenHashAsync("hash", Arg.Any<CancellationToken>()).Returns(stored);
        _users.GetByIdAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(_user);
        _jwt.GenerateRefreshToken().Returns("new_raw");
        _tokenHasher.Hash("new_raw").Returns("new_hash");
        _jwt.GenerateAccessToken(_user).Returns("access");
        _config.RefreshTokenDays.Returns(7);

        var result = await CreateHandler().HandleAsync(new RefreshCommand("raw"));

        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("new_raw");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_InvalidToken_ThrowsUnauthorized()
    {
        _tokenHasher.Hash(Arg.Any<string>()).Returns("hash");
        _refreshTokens.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var act = () => CreateHandler().HandleAsync(new RefreshCommand("invalid"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task HandleAsync_RevokedToken_ThrowsUnauthorized()
    {
        var stored = RefreshToken.Create(_user.Id, "hash", DateTime.UtcNow.AddDays(7));
        stored.Revoke();
        _tokenHasher.Hash(Arg.Any<string>()).Returns("hash");
        _refreshTokens.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(stored);

        var act = () => CreateHandler().HandleAsync(new RefreshCommand("raw"));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
