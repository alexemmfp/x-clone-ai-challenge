using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Repositories;

internal sealed class TweetRepository(AppDbContext db) : ITweetRepository
{
    public Task<Tweet?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Tweets.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(Tweet tweet, CancellationToken ct = default) =>
        await db.Tweets.AddAsync(tweet, ct);

    public Task RemoveAsync(Tweet tweet, CancellationToken ct = default)
    {
        db.Tweets.Remove(tweet);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Tweet>> GetTimelineAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        var followedIds = await db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FolloweeId)
            .ToListAsync(ct);

        followedIds.Add(userId);

        return await db.Tweets
            .Where(t => followedIds.Contains(t.AuthorId) && t.ParentId == null)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Tweet>> GetRepliesAsync(Guid parentId, CancellationToken ct = default) =>
        await db.Tweets
            .Where(t => t.ParentId == parentId)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(ct);
}
