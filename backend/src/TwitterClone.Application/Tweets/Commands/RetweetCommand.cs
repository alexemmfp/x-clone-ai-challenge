using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Tweets.Commands;

public sealed record RetweetCommand(Guid RetweeterId, Guid TweetId);

public sealed class RetweetHandler(IRetweetRepository retweets, ITweetRepository tweets, IUnitOfWork uow)
{
    public async Task HandleAsync(RetweetCommand cmd, CancellationToken ct = default)
    {
        var already = await retweets.ExistsAsync(cmd.RetweeterId, cmd.TweetId, ct);
        if (already) { return; }

        var tweet = await tweets.GetByIdAsync(cmd.TweetId, ct)
            ?? throw new InvalidOperationException("Tweet not found.");

        var retweet = Retweet.Create(cmd.RetweeterId, cmd.TweetId, tweet.AuthorId);
        await retweets.AddAsync(retweet, ct);
        await uow.SaveChangesAsync(ct);
    }
}
