using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Queries;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Tweets;

public class GetRepliesHandlerTests
{
    private readonly ITweetRepository _tweets = Substitute.For<ITweetRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ILikeRepository _likes = Substitute.For<ILikeRepository>();

    private readonly User _author = User.Create("bob", "bob@example.com", "hash");

    private GetRepliesHandler CreateHandler() => new(_tweets, _users, _likes);

    [Fact]
    public async Task HandleAsync_WithReplies_ReturnsDtoList()
    {
        var parentId = Guid.NewGuid();
        var reply = Tweet.Create(_author.Id, "A reply", parentId);

        _tweets.GetRepliesAsync(parentId).Returns([reply]);
        _users.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, User> { [_author.Id] = _author });
        _likes.CountForTweetsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int> { [reply.Id] = 0 });
        _likes.GetLikedByUserAsync(Arg.Any<Guid>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<Guid>());
        _tweets.GetReplyCountsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, int>());

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetRepliesQuery(Guid.NewGuid(), parentId));

        result.Should().HaveCount(1);
        result[0].ParentId.Should().Be(parentId);
        result[0].AuthorUsername.Should().Be("bob");
    }

    [Fact]
    public async Task HandleAsync_NoReplies_ReturnsEmptyList()
    {
        var parentId = Guid.NewGuid();
        _tweets.GetRepliesAsync(parentId).Returns([]);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new GetRepliesQuery(Guid.NewGuid(), parentId));

        result.Should().BeEmpty();
    }
}
