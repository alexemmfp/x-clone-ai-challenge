using FluentAssertions;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_ValidArgs_ReturnsUser()
    {
        var user = User.Create("alice", "alice@example.com", "hashedpw");
        user.Username.Should().Be("alice");
        user.Email.Should().Be("alice@example.com");
        user.PasswordHash.Should().Be("hashedpw");
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_EmptyUsername_ThrowsDomainException(string username)
    {
        var act = () => User.Create(username, "a@b.com", "hash");
        act.Should().Throw<DomainException>().WithMessage("*username*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Create_InvalidEmail_ThrowsDomainException(string email)
    {
        var act = () => User.Create("alice", email, "hash");
        act.Should().Throw<DomainException>().WithMessage("*email*");
    }
}
