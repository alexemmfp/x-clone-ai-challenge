using FluentAssertions;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Tests.Entities;

public class TweetTests
{
    private static readonly Guid AuthorId = Guid.NewGuid();

    [Fact]
    public void Create_ValidText_ReturnsTweet()
    {
        var tweet = Tweet.Create(AuthorId, "Hello world");
        tweet.Text.Should().Be("Hello world");
        tweet.AuthorId.Should().Be(AuthorId);
        tweet.ParentId.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyText_ThrowsDomainException(string text)
    {
        var act = () => Tweet.Create(AuthorId, text);
        act.Should().Throw<DomainException>().WithMessage("*text*");
    }

    [Fact]
    public void Create_TextOver280Chars_ThrowsDomainException()
    {
        var text = new string('x', 281);
        var act = () => Tweet.Create(AuthorId, text);
        act.Should().Throw<DomainException>().WithMessage("*280*");
    }

    [Fact]
    public void Create_TextExactly280Chars_Succeeds()
    {
        var text = new string('x', 280);
        var act = () => Tweet.Create(AuthorId, text);
        act.Should().NotThrow();
    }
}
