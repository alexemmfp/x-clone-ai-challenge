using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Repositories;

internal sealed class RetweetRepository(AppDbContext db) : IRetweetRepository
{
    public Task<bool> ExistsAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default) =>
        db.Retweets.AnyAsync(r => r.RetweeterId == retweeterId && r.TweetId == tweetId, ct);

    public async Task AddAsync(Retweet retweet, CancellationToken ct = default) =>
        await db.Retweets.AddAsync(retweet, ct);

    public async Task RemoveAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default)
    {
        var rt = await db.Retweets
            .FirstOrDefaultAsync(r => r.RetweeterId == retweeterId && r.TweetId == tweetId, ct);
        if (rt is not null) { db.Retweets.Remove(rt); }
    }

    public Task<int> CountAsync(Guid tweetId, CancellationToken ct = default) =>
        db.Retweets.CountAsync(r => r.TweetId == tweetId, ct);

    public async Task<IReadOnlyList<(Tweet Tweet, string RetweeterUsername, DateTime RetweetedAt)>>
        GetTimelineRetweetsAsync(Guid viewerId, int count, CancellationToken ct = default)
    {
        var followedIds = await db.Follows
            .Where(f => f.FollowerId == viewerId)
            .Select(f => f.FolloweeId)
            .ToListAsync(ct);

        followedIds.Add(viewerId);

        var rows = await db.Retweets
            .Where(r => followedIds.Contains(r.RetweeterId))
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .Join(db.Tweets, r => r.TweetId, t => t.Id, (r, t) => new { r, t })
            .Join(db.Users, x => x.r.RetweeterId, u => u.Id,
                (x, u) => new { x.t, RetweeterUsername = u.Username, x.r.CreatedAt })
            .ToListAsync(ct);

        return rows.Select(x => (x.t, x.RetweeterUsername, x.CreatedAt)).ToList();
    }
}
