using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Commands;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Tests.Tweets;

public class RetweetHandlerTests
{
    private readonly IRetweetRepository _retweets = Substitute.For<IRetweetRepository>();
    private readonly ITweetRepository _tweets = Substitute.For<ITweetRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task RetweetHandler_NewRetweet_AddsAndSaves()
    {
        var retweeterId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var tweet = CreateTweet(authorId);

        _tweets.GetByIdAsync(tweet.Id, Arg.Any<CancellationToken>()).Returns(tweet);
        _retweets.ExistsAsync(retweeterId, tweet.Id, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new RetweetHandler(_retweets, _tweets, _uow);
        await handler.HandleAsync(new RetweetCommand(retweeterId, tweet.Id));

        await _retweets.Received(1).AddAsync(Arg.Any<Retweet>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetweetHandler_AlreadyRetweeted_DoesNotDuplicate()
    {
        var retweeterId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var tweet = CreateTweet(authorId);

        _tweets.GetByIdAsync(tweet.Id, Arg.Any<CancellationToken>()).Returns(tweet);
        _retweets.ExistsAsync(retweeterId, tweet.Id, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new RetweetHandler(_retweets, _tweets, _uow);
        await handler.HandleAsync(new RetweetCommand(retweeterId, tweet.Id));

        await _retweets.DidNotReceive().AddAsync(Arg.Any<Retweet>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetweetHandler_OwnTweet_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var tweet = CreateTweet(userId);

        _tweets.GetByIdAsync(tweet.Id, Arg.Any<CancellationToken>()).Returns(tweet);
        _retweets.ExistsAsync(userId, tweet.Id, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new RetweetHandler(_retweets, _tweets, _uow);
        var act = async () => await handler.HandleAsync(new RetweetCommand(userId, tweet.Id));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UnretweetHandler_ExistingRetweet_Removes()
    {
        var retweeterId = Guid.NewGuid();
        var tweetId = Guid.NewGuid();
        _retweets.ExistsAsync(retweeterId, tweetId, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new UnretweetHandler(_retweets, _uow);
        await handler.HandleAsync(new UnretweetCommand(retweeterId, tweetId));

        await _retweets.Received(1).RemoveAsync(retweeterId, tweetId, Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Tweet CreateTweet(Guid authorId) =>
        Tweet.Create(authorId, "test tweet");
}
