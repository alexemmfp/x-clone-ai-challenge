using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Tweets.Commands;

public sealed record UnretweetCommand(Guid RetweeterId, Guid TweetId);

public sealed class UnretweetHandler(IRetweetRepository retweets, IUnitOfWork uow)
{
    public async Task HandleAsync(UnretweetCommand cmd, CancellationToken ct = default)
    {
        var exists = await retweets.ExistsAsync(cmd.RetweeterId, cmd.TweetId, ct);
        if (!exists) { return; }

        await retweets.RemoveAsync(cmd.RetweeterId, cmd.TweetId, ct);
        await uow.SaveChangesAsync(ct);
    }
}
