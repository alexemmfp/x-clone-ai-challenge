using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Search.Queries;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Search;

public class SearchUsersHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();

    private SearchUsersHandler CreateHandler() => new(_users);

    [Fact]
    public async Task HandleAsync_MatchingUsers_ReturnsProfileDtos()
    {
        var results = new List<User> { User.Create("alice", "alice@example.com", "h") };
        _users.SearchAsync("ali", 10, Arg.Any<CancellationToken>()).Returns(results);

        var dtos = await CreateHandler().HandleAsync(new SearchUsersQuery("ali"));

        dtos.Should().HaveCount(1);
        dtos[0].Username.Should().Be("alice");
    }

    [Fact]
    public async Task HandleAsync_NoResults_ReturnsEmpty()
    {
        _users.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<User>());

        var dtos = await CreateHandler().HandleAsync(new SearchUsersQuery("xyz"));

        dtos.Should().BeEmpty();
    }
}
