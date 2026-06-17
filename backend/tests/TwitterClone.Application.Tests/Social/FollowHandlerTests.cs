using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Social.Commands;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Tests.Social;

public class FollowHandlerTests
{
    private readonly IFollowRepository _follows = Substitute.For<IFollowRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private readonly User _follower = User.Create("alice", "alice@example.com", "hash");
    private readonly User _followee = User.Create("bob", "bob@example.com", "hash");

    private FollowHandler CreateHandler() => new(_follows, _users, _uow);

    [Fact]
    public async Task HandleAsync_ValidFollow_SavesFollow()
    {
        _users.GetByIdAsync(_followee.Id).Returns(_followee);
        _follows.GetAsync(_follower.Id, _followee.Id).Returns((Follow?)null);

        await CreateHandler().HandleAsync(new FollowCommand(_follower.Id, _followee.Id));

        await _follows.Received(1).AddAsync(Arg.Any<Follow>());
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_FollowSelf_ThrowsDomainException()
    {
        var act = () => CreateHandler().HandleAsync(new FollowCommand(_follower.Id, _follower.Id));
        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task HandleAsync_AlreadyFollowing_IsIdempotent()
    {
        _users.GetByIdAsync(_followee.Id).Returns(_followee);
        _follows.GetAsync(_follower.Id, _followee.Id).Returns(Follow.Create(_follower.Id, _followee.Id));

        await CreateHandler().HandleAsync(new FollowCommand(_follower.Id, _followee.Id));

        await _follows.DidNotReceive().AddAsync(Arg.Any<Follow>());
        await _uow.DidNotReceive().SaveChangesAsync();
    }
}
