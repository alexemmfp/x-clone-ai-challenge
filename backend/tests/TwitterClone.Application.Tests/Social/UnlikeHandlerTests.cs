using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Social.Commands;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Social;

public class UnlikeHandlerTests
{
    private readonly ILikeRepository _likes = Substitute.For<ILikeRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private UnlikeHandler CreateHandler() => new(_likes, _uow);

    [Fact]
    public async Task HandleAsync_ExistingLike_Removes()
    {
        var userId = Guid.NewGuid();
        var tweetId = Guid.NewGuid();
        var like = Like.Create(userId, tweetId);
        _likes.GetAsync(userId, tweetId, Arg.Any<CancellationToken>()).Returns(like);

        await CreateHandler().HandleAsync(new UnlikeCommand(userId, tweetId));

        await _likes.Received(1).RemoveAsync(like, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_NotLiked_IsIdempotent()
    {
        _likes.GetAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Like?)null);

        await CreateHandler().HandleAsync(new UnlikeCommand(Guid.NewGuid(), Guid.NewGuid()));

        await _likes.DidNotReceive().RemoveAsync(Arg.Any<Like>(), Arg.Any<CancellationToken>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
