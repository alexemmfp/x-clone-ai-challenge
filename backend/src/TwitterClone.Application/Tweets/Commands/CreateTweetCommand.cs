using FluentValidation;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tweets.Commands;

public sealed record CreateTweetCommand(Guid AuthorId, string Text, Guid? ParentId = null, string? ImageUrl = null);

public sealed class CreateTweetCommandValidator : AbstractValidator<CreateTweetCommand>
{
    public CreateTweetCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(280);
    }
}

public sealed class CreateTweetHandler(
    ITweetRepository tweets,
    IUserRepository users,
    IUnitOfWork uow,
    ITimelineNotifier notifier)
{
    public async Task<TweetDto> HandleAsync(CreateTweetCommand cmd, CancellationToken ct = default)
    {
        var author = await users.GetByIdAsync(cmd.AuthorId, ct)
            ?? throw new InvalidOperationException("User not found.");

        var tweet = Tweet.Create(cmd.AuthorId, cmd.Text, cmd.ParentId, cmd.ImageUrl);
        await tweets.AddAsync(tweet, ct);
        await uow.SaveChangesAsync(ct);

        var dto = new TweetDto(tweet.Id, tweet.AuthorId, author.Username, tweet.Text, tweet.ParentId, tweet.ImageUrl, tweet.CreatedAt);
        await notifier.NotifyTweetCreatedAsync(dto, ct);
        return dto;
    }
}
