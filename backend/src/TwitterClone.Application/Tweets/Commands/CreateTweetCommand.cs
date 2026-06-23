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

        Guid? replyNotifiedUserId = null;
        if (cmd.ParentId is not null)
        {
            var parentTweet = await tweets.GetByIdAsync(cmd.ParentId.Value, ct);
            if (parentTweet is not null && parentTweet.AuthorId != cmd.AuthorId)
            {
                await notifier.NotifyRepliedAsync(parentTweet.AuthorId, parentTweet.Id, author.Username, cmd.Text, ct);
                replyNotifiedUserId = parentTweet.AuthorId;
            }
        }

        var mentionMatches = System.Text.RegularExpressions.Regex.Matches(cmd.Text, @"@(\w+)");
        var mentioned = mentionMatches.Select(m => m.Groups[1].Value).Distinct().ToList();
        if (mentioned.Count > 0)
        {
            var existing = await users.GetExistingUsernamesAsync(mentioned, ct);
            foreach (var uname in existing)
            {
                var u = await users.GetByUsernameAsync(uname, ct);
                if (u is not null && u.Id != cmd.AuthorId && u.Id != replyNotifiedUserId)
                {
                    await notifier.NotifyMentionedAsync(u.Id, tweet.Id, author.Username, cmd.Text, ct);
                }
            }
        }

        var dto = new TweetDto(tweet.Id, tweet.AuthorId, author.Username, tweet.Text, tweet.ParentId, tweet.ImageUrl, tweet.CreatedAt);
        await notifier.NotifyTweetCreatedAsync(dto, ct);
        return dto;
    }
}
