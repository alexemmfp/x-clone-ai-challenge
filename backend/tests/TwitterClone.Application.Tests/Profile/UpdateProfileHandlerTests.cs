using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Profile.Commands;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Tests.Profile;

public class UpdateProfileHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IFollowRepository _follows = Substitute.For<IFollowRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private readonly User _user = User.Create("alice", "alice@example.com", "hash");

    private UpdateProfileHandler CreateHandler() => new(_users, _follows, _uow);

    [Fact]
    public async Task HandleAsync_ValidUpdate_ReturnsUpdatedProfile()
    {
        _users.GetByIdAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(_user);
        _follows.CountFollowersAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(0);
        _follows.CountFollowingAsync(_user.Id, Arg.Any<CancellationToken>()).Returns(0);

        var result = await CreateHandler().HandleAsync(new UpdateProfileCommand(_user.Id, "new bio", null));

        result.Bio.Should().Be("new bio");
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ThrowsDomainException()
    {
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var act = () => CreateHandler().HandleAsync(new UpdateProfileCommand(Guid.NewGuid(), "bio", null));

        await act.Should().ThrowAsync<DomainException>();
    }
}
