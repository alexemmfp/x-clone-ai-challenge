using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Social.Commands;

public sealed record LikeCommand(Guid UserId, Guid TweetId);

public sealed class LikeHandler(
    ILikeRepository likes,
    ITweetRepository tweets,
    IUnitOfWork uow)
{
    public async Task HandleAsync(LikeCommand cmd, CancellationToken ct = default)
    {
        var tweet = await tweets.GetByIdAsync(cmd.TweetId, ct)
            ?? throw new DomainException("Tweet not found.");

        var existing = await likes.GetAsync(cmd.UserId, cmd.TweetId, ct);
        if (existing is not null)
        {
            return;
        }

        await likes.AddAsync(Like.Create(cmd.UserId, cmd.TweetId), ct);
        await uow.SaveChangesAsync(ct);

        _ = tweet;
    }
}
