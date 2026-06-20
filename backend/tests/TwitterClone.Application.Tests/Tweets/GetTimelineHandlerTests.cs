using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Queries;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Tweets;

public class GetTimelineHandlerTests
{
    private readonly ITweetRepository _tweets = Substitute.For<ITweetRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ILikeRepository _likes = Substitute.For<ILikeRepository>();
    private readonly IRetweetRepository _retweets = Substitute.For<IRetweetRepository>();

    private readonly User _author = User.Create("alice", "alice@example.com", "hash");

    private GetTimelineHandler CreateHandler() => new(_tweets, _users, _likes, _retweets);

    [Fact]
    public async Task HandleAsync_WithTweets_ReturnsDtos()
    {
        var viewerId = Guid.NewGuid();
        var tweet = Tweet.Create(_author.Id, "hello world");
        _tweets.GetTimelineAsync(viewerId, 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Tweet> { tweet });
        _retweets.GetTimelineRetweetsAsync(viewerId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Tweet, string, DateTime)>());
        _users.GetByIdAsync(_author.Id, Arg.Any<CancellationToken>()).Returns(_author);
        _likes.CountAsync(tweet.Id, Arg.Any<CancellationToken>()).Returns(2);
        _likes.GetAsync(viewerId, tweet.Id, Arg.Any<CancellationToken>()).Returns((Like?)null);

        var result = await CreateHandler().HandleAsync(new GetTimelineQuery(viewerId));

        result.Should().HaveCount(1);
        result[0].Text.Should().Be("hello world");
        result[0].LikeCount.Should().Be(2);
        result[0].LikedByViewer.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_EmptyTimeline_ReturnsEmpty()
    {
        _tweets.GetTimelineAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Tweet>());
        _retweets.GetTimelineRetweetsAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Tweet, string, DateTime)>());

        var result = await CreateHandler().HandleAsync(new GetTimelineQuery(Guid.NewGuid()));

        result.Should().BeEmpty();
    }
}
