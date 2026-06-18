using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Profile.Queries;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Tests.Profile;

public class GetProfileHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IFollowRepository _follows = Substitute.For<IFollowRepository>();

    private readonly User _user = User.Create("alice", "alice@example.com", "hash");

    private GetProfileHandler CreateHandler() => new(_users, _follows);

    [Fact]
    public async Task HandleAsync_ExistingUser_ReturnsProfileDto()
    {
        _users.GetByUsernameAsync("alice", Arg.Any<CancellationToken>()).Returns(_user);
        _follows.CountFollowersAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(3);
        _follows.CountFollowingAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(5);

        var result = await CreateHandler().HandleAsync(new GetProfileQuery("alice"));

        result.Username.Should().Be("alice");
        result.FollowerCount.Should().Be(3);
        result.FollowingCount.Should().Be(5);
        result.IsFollowedByViewer.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithViewer_SetsIsFollowed()
    {
        var viewer = User.Create("bob", "bob@example.com", "hash");
        var follow = Follow.Create(viewer.Id, _user.Id);
        _users.GetByUsernameAsync("alice", Arg.Any<CancellationToken>()).Returns(_user);
        _follows.CountFollowersAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(1);
        _follows.CountFollowingAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(0);
        _follows.GetAsync(viewer.Id, _user.Id, Arg.Any<CancellationToken>()).Returns(follow);

        var result = await CreateHandler().HandleAsync(new GetProfileQuery("alice", viewer.Id));

        result.IsFollowedByViewer.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ThrowsDomainException()
    {
        _users.GetByUsernameAsync("nobody", Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => CreateHandler().HandleAsync(new GetProfileQuery("nobody"));

        await act.Should().ThrowAsync<DomainException>();
    }
}
