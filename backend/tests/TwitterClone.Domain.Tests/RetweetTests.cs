using FluentAssertions;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Tests;

public class RetweetTests
{
    [Fact]
    public void Create_ValidIds_SetsProperties()
    {
        var retweeterId = Guid.NewGuid();
        var tweetId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var rt = Retweet.Create(retweeterId, tweetId, authorId);

        rt.RetweeterId.Should().Be(retweeterId);
        rt.TweetId.Should().Be(tweetId);
        rt.Id.Should().NotBeEmpty();
        rt.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_RetweeterIsAuthor_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var tweetId = Guid.NewGuid();

        var act = () => Retweet.Create(userId, tweetId, authorId: userId);

        act.Should().Throw<DomainException>().WithMessage("*own tweet*");
    }
}
