using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Social.Commands;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Social;

public class UnfollowHandlerTests
{
    private readonly IFollowRepository _follows = Substitute.For<IFollowRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private UnfollowHandler CreateHandler() => new(_follows, _uow);

    [Fact]
    public async Task HandleAsync_ExistingFollow_Removes()
    {
        var followerId = Guid.NewGuid();
        var followeeId = Guid.NewGuid();
        var follow = Follow.Create(followerId, followeeId);
        _follows.GetAsync(followerId, followeeId, Arg.Any<CancellationToken>()).Returns(follow);

        await CreateHandler().HandleAsync(new UnfollowCommand(followerId, followeeId));

        await _follows.Received(1).RemoveAsync(follow, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NotFollowing_IsIdempotent()
    {
        _follows.GetAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Follow?)null);

        await CreateHandler().HandleAsync(new UnfollowCommand(Guid.NewGuid(), Guid.NewGuid()));

        await _follows.DidNotReceive().RemoveAsync(Arg.Any<Follow>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
