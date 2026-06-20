using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Users.Queries;

namespace TwitterClone.Application.Tests.Users;

public class ValidateUsernamesQueryTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ValidateUsernamesHandler _handler;

    public ValidateUsernamesQueryTests()
    {
        _handler = new ValidateUsernamesHandler(_users);
    }

    [Fact]
    public async Task HandleAsync_ReturnsCorrectBoolMap()
    {
        _users.GetExistingUsernamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "alice", "bob" });

        var result = await _handler.HandleAsync(
            new ValidateUsernamesQuery(["alice", "bob", "nobody"]), CancellationToken.None);

        result.Should().BeEquivalentTo(new Dictionary<string, bool>
        {
            ["alice"] = true,
            ["bob"] = true,
            ["nobody"] = false,
        });
    }

    [Fact]
    public async Task HandleAsync_EmptyInput_ReturnsEmptyMap()
    {
        _users.GetExistingUsernamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>());

        var result = await _handler.HandleAsync(new ValidateUsernamesQuery([]), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_TruncatesOver50Usernames()
    {
        var many = Enumerable.Range(1, 60).Select(i => $"user{i}").ToArray();
        _users.GetExistingUsernamesAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<string>());

        await _handler.HandleAsync(new ValidateUsernamesQuery(many), CancellationToken.None);

        await _users.Received(1).GetExistingUsernamesAsync(
            Arg.Is<IEnumerable<string>>(u => u.Count() <= 50), Arg.Any<CancellationToken>());
    }
}
