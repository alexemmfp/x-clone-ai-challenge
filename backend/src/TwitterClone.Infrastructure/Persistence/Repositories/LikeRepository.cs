#pragma warning disable CA1716
using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Repositories;

internal sealed class LikeRepository(AppDbContext db) : ILikeRepository
{
    public Task<Like?> GetAsync(Guid userId, Guid tweetId, CancellationToken ct = default) =>
        db.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.TweetId == tweetId, ct);

    public async Task AddAsync(Like like, CancellationToken ct = default) =>
        await db.Likes.AddAsync(like, ct);

    public Task RemoveAsync(Like like, CancellationToken ct = default)
    {
        db.Likes.Remove(like);
        return Task.CompletedTask;
    }

    public Task<int> CountAsync(Guid tweetId, CancellationToken ct = default) =>
        db.Likes.CountAsync(l => l.TweetId == tweetId, ct);

    public async Task<IReadOnlyDictionary<Guid, int>> CountForTweetsAsync(
        IEnumerable<Guid> tweetIds, CancellationToken ct = default)
    {
        var list = tweetIds.ToList();
        var counts = await db.Likes
            .Where(l => list.Contains(l.TweetId))
            .GroupBy(l => l.TweetId)
            .Select(g => new { TweetId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var dict = counts.ToDictionary(x => x.TweetId, x => x.Count);
        foreach (var id in list.Where(id => !dict.ContainsKey(id)))
        {
            dict[id] = 0;
        }
        return dict;
    }

    public async Task<IReadOnlySet<Guid>> GetLikedByUserAsync(
        Guid userId, IEnumerable<Guid> tweetIds, CancellationToken ct = default)
    {
        var list = tweetIds.ToList();
        var liked = await db.Likes
            .Where(l => l.UserId == userId && list.Contains(l.TweetId))
            .Select(l => l.TweetId)
            .ToListAsync(ct);
        return new HashSet<Guid>(liked);
    }
}
#pragma warning restore CA1716
