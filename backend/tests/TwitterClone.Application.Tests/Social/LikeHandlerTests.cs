using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Social.Commands;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Social;

public class LikeHandlerTests
{
    private readonly ILikeRepository _likes = Substitute.For<ILikeRepository>();
    private readonly ITweetRepository _tweets = Substitute.For<ITweetRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ITimelineNotifier _notifier = Substitute.For<ITimelineNotifier>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private readonly User _user = User.Create("alice", "alice@example.com", "hash");
    private readonly Tweet _tweet = Tweet.Create(Guid.NewGuid(), "hello");

    private LikeHandler CreateHandler() => new(_likes, _tweets, _users, _notifier, _uow);

    [Fact]
    public async Task HandleAsync_ValidLike_SavesLike()
    {
        _tweets.GetByIdAsync(_tweet.Id).Returns(_tweet);
        _likes.GetAsync(_user.Id, _tweet.Id).Returns((Like?)null);

        await CreateHandler().HandleAsync(new LikeCommand(_user.Id, _tweet.Id));

        await _likes.Received(1).AddAsync(Arg.Any<Like>());
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_AlreadyLiked_IsIdempotent()
    {
        _tweets.GetByIdAsync(_tweet.Id).Returns(_tweet);
        _likes.GetAsync(_user.Id, _tweet.Id).Returns(Like.Create(_user.Id, _tweet.Id));

        await CreateHandler().HandleAsync(new LikeCommand(_user.Id, _tweet.Id));

        await _likes.DidNotReceive().AddAsync(Arg.Any<Like>());
        await _uow.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_TweetNotFound_ThrowsDomainException()
    {
        _tweets.GetByIdAsync(Arg.Any<Guid>()).Returns((Tweet?)null);

        var act = () => CreateHandler().HandleAsync(new LikeCommand(_user.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<TwitterClone.Domain.Exceptions.DomainException>();
    }
}
