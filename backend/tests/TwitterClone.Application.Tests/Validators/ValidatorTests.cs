using FluentAssertions;
using TwitterClone.Application.Auth.Commands;
using TwitterClone.Application.Profile.Commands;
using TwitterClone.Application.Tweets.Commands;

namespace TwitterClone.Application.Tests.Validators;

public class ValidatorTests
{
    [Theory]
    [InlineData("valid@email.com", "password123", true)]
    [InlineData("", "password123", false)]
    [InlineData("not-an-email", "password123", false)]
    [InlineData("valid@email.com", "", false)]
    public async Task LoginCommandValidator_ValidatesCorrectly(string email, string password, bool expected)
    {
        var validator = new LoginCommandValidator();
        var result = await validator.ValidateAsync(new LoginCommand(email, password));
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("alice", "a@b.com", "Password1!", true)]
    [InlineData("", "a@b.com", "Password1!", false)]
    [InlineData("ab", "a@b.com", "Password1!", false)]
    [InlineData("alice", "not-email", "Password1!", false)]
    [InlineData("alice", "a@b.com", "short", false)]
    [InlineData("bad user!", "a@b.com", "Password1!", false)]
    public async Task RegisterCommandValidator_ValidatesCorrectly(string username, string email, string password, bool expected)
    {
        var validator = new RegisterCommandValidator();
        var result = await validator.ValidateAsync(new RegisterCommand(username, email, password));
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData("Hello world", true)]
    [InlineData("", false)]
    public async Task CreateTweetCommandValidator_ValidatesCorrectly(string text, bool expected)
    {
        var validator = new CreateTweetCommandValidator();
        var result = await validator.ValidateAsync(new CreateTweetCommand(Guid.NewGuid(), text));
        result.IsValid.Should().Be(expected);
    }

    [Fact]
    public async Task CreateTweetCommandValidator_TextTooLong_Invalid()
    {
        var validator = new CreateTweetCommandValidator();
        var result = await validator.ValidateAsync(new CreateTweetCommand(Guid.NewGuid(), new string('a', 281)));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfileCommandValidator_BioTooLong_Invalid()
    {
        var validator = new UpdateProfileCommandValidator();
        var result = await validator.ValidateAsync(new UpdateProfileCommand(Guid.NewGuid(), new string('a', 161), null));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateProfileCommandValidator_ValidBio_Valid()
    {
        var validator = new UpdateProfileCommandValidator();
        var result = await validator.ValidateAsync(new UpdateProfileCommand(Guid.NewGuid(), "short bio", null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("/uploads/abc.png", true)]
    [InlineData("https://cdn.example.com/img.jpg", true)]
    [InlineData("http://example.com/img.png", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("data:image/png;base64,abc", false)]
    [InlineData("ftp://example.com/img.png", false)]
    public async Task CreateTweetCommandValidator_ImageUrl_ValidatesCorrectly(string? imageUrl, bool expected)
    {
        var validator = new CreateTweetCommandValidator();
        var result = await validator.ValidateAsync(new CreateTweetCommand(Guid.NewGuid(), "Hello", ImageUrl: imageUrl));
        result.IsValid.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("/uploads/avatar.jpg", true)]
    [InlineData("https://cdn.example.com/avatar.png", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("data:image/png;base64,abc", false)]
    public async Task UpdateProfileCommandValidator_AvatarUrl_ValidatesCorrectly(string? avatarUrl, bool expected)
    {
        var validator = new UpdateProfileCommandValidator();
        var result = await validator.ValidateAsync(new UpdateProfileCommand(Guid.NewGuid(), "bio", avatarUrl));
        result.IsValid.Should().Be(expected);
    }
}
