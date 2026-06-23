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

    private void SetupEmptyBatchDefaults()
    {
        _users.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, User>());
        _likes.CountForTweetsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
        _likes.GetLikedByUserAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());
        _retweets.CountForTweetsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
        _retweets.GetRetweetedByUserAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());
        _tweets.GetReplyCountsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
    }

    [Fact]
    public async Task HandleAsync_WithTweets_ReturnsDtos()
    {
        var viewerId = Guid.NewGuid();
        var tweet = Tweet.Create(_author.Id, "hello world");
        _tweets.GetTimelineAsync(viewerId, 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Tweet> { tweet });
        _retweets.GetTimelineRetweetsAsync(viewerId, 1, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Tweet, string, DateTime)>());

        _users.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, User> { [_author.Id] = _author });
        _likes.CountForTweetsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [tweet.Id] = 2 });
        _likes.GetLikedByUserAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());
        _retweets.CountForTweetsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());
        _retweets.GetRetweetedByUserAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());
        _tweets.GetReplyCountsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());

        var result = await CreateHandler().HandleAsync(new GetTimelineQuery(viewerId));

        result.Should().HaveCount(1);
        result[0].Text.Should().Be("hello world");
        result[0].LikeCount.Should().Be(2);
        result[0].LikedByViewer.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_PageTwo_PassesPageToRepository()
    {
        var viewerId = Guid.NewGuid();
        _tweets.GetTimelineAsync(viewerId, 2, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Tweet>());
        _retweets.GetTimelineRetweetsAsync(viewerId, 2, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Tweet, string, DateTime)>());
        SetupEmptyBatchDefaults();

        await CreateHandler().HandleAsync(new GetTimelineQuery(viewerId, Page: 2));

        await _tweets.Received(1).GetTimelineAsync(viewerId, 2, Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _retweets.Received(1).GetTimelineRetweetsAsync(viewerId, 2, Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_EmptyTimeline_ReturnsEmpty()
    {
        _tweets.GetTimelineAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Tweet>());
        _retweets.GetTimelineRetweetsAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<(Tweet, string, DateTime)>());
        SetupEmptyBatchDefaults();

        var result = await CreateHandler().HandleAsync(new GetTimelineQuery(Guid.NewGuid()));

        result.Should().BeEmpty();
    }
}
