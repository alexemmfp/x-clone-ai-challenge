using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Commands;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tests.Tweets;

public class CreateTweetHandlerTests
{
    private readonly ITweetRepository _tweets = Substitute.For<ITweetRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ITimelineNotifier _notifier = Substitute.For<ITimelineNotifier>();

    private readonly User _author = User.Create("alice", "alice@example.com", "hash");

    private CreateTweetHandler CreateHandler() => new(_tweets, _users, _uow, _notifier);

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsTweetDto()
    {
        _users.GetByIdAsync(_author.Id).Returns(_author);

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CreateTweetCommand(_author.Id, "Hello world!"));

        result.Text.Should().Be("Hello world!");
        result.AuthorUsername.Should().Be("alice");
        await _tweets.Received(1).AddAsync(Arg.Any<Tweet>());
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task HandleAsync_UnknownAuthor_ThrowsInvalidOperationException()
    {
        _users.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        var handler = CreateHandler();
        var act = () => handler.HandleAsync(new CreateTweetCommand(Guid.NewGuid(), "text"));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_WithParentId_SetsTweetParent()
    {
        _users.GetByIdAsync(_author.Id).Returns(_author);
        var parentId = Guid.NewGuid();

        var handler = CreateHandler();
        var result = await handler.HandleAsync(new CreateTweetCommand(_author.Id, "Reply!", parentId));

        result.ParentId.Should().Be(parentId);
    }
}
