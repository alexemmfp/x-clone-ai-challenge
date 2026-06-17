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
}
#pragma warning restore CA1716
