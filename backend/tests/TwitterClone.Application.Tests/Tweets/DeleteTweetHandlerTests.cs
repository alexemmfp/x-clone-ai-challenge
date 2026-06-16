using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Commands;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Tests.Tweets;

public class DeleteTweetHandlerTests
{
    private readonly ITweetRepository _tweets = Substitute.For<ITweetRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private readonly User _author = User.Create("alice", "alice@example.com", "hash");

    private DeleteTweetHandler CreateHandler() => new(_tweets, _uow);

    [Fact]
    public async Task HandleAsync_OwnTweet_Deletes()
    {
        var tweet = Tweet.Create(_author.Id, "to delete");
        _tweets.GetByIdAsync(tweet.Id).Returns(tweet);

        var handler = CreateHandler();
        await handler.HandleAsync(new DeleteTweetCommand(tweet.Id, _author.Id));

        await _tweets.Received(1).RemoveAsync(tweet);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_NotFound_ThrowsDomainException()
    {
        _tweets.GetByIdAsync(Arg.Any<Guid>()).Returns((Tweet?)null);

        var handler = CreateHandler();
        var act = () => handler.HandleAsync(new DeleteTweetCommand(Guid.NewGuid(), _author.Id));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task HandleAsync_WrongUser_ThrowsUnauthorizedAccessException()
    {
        var tweet = Tweet.Create(_author.Id, "text");
        _tweets.GetByIdAsync(tweet.Id).Returns(tweet);

        var handler = CreateHandler();
        var act = () => handler.HandleAsync(new DeleteTweetCommand(tweet.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
