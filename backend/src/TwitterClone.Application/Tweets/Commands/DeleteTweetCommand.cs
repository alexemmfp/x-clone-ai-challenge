using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Tweets.Commands;

public sealed record DeleteTweetCommand(Guid TweetId, Guid RequestingUserId);

public sealed class DeleteTweetHandler(ITweetRepository tweets, IUnitOfWork uow)
{
    public async Task HandleAsync(DeleteTweetCommand cmd, CancellationToken ct = default)
    {
        var tweet = await tweets.GetByIdAsync(cmd.TweetId, ct)
            ?? throw new DomainException("Tweet not found.");

        if (tweet.AuthorId != cmd.RequestingUserId)
        {
            throw new UnauthorizedAccessException("Cannot delete another user's tweet.");
        }

        await tweets.RemoveAsync(tweet, ct);
        await uow.SaveChangesAsync(ct);
    }
}
