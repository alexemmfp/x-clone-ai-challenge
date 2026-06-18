using NSubstitute;
using TwitterClone.Application.Auth.Commands;
using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Tests.Auth;

public class LogoutHandlerTests
{
    private readonly IRefreshTokenRepository _refreshTokens = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private LogoutHandler CreateHandler() => new(_refreshTokens, _uow);

    [Fact]
    public async Task HandleAsync_RevokesAllTokensForUser()
    {
        var userId = Guid.NewGuid();

        await CreateHandler().HandleAsync(new LogoutCommand(userId));

        await _refreshTokens.Received(1).RevokeAllForUserAsync(userId, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
