using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Repositories;

internal sealed class FollowRepository(AppDbContext db) : IFollowRepository
{
    public Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken ct = default) =>
        db.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, ct);

    public async Task AddAsync(Follow follow, CancellationToken ct = default) =>
        await db.Follows.AddAsync(follow, ct);

    public Task RemoveAsync(Follow follow, CancellationToken ct = default)
    {
        db.Follows.Remove(follow);
        return Task.CompletedTask;
    }

    public Task<int> CountFollowersAsync(Guid userId, CancellationToken ct = default) =>
        db.Follows.CountAsync(f => f.FolloweeId == userId, ct);

    public Task<int> CountFollowingAsync(Guid userId, CancellationToken ct = default) =>
        db.Follows.CountAsync(f => f.FollowerId == userId, ct);

    public async Task<IReadOnlyList<Guid>> GetFollowerIdsAsync(Guid userId, CancellationToken ct = default) =>
        await db.Follows.Where(f => f.FolloweeId == userId).Select(f => f.FollowerId).ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid userId, CancellationToken ct = default) =>
        await db.Follows.Where(f => f.FollowerId == userId).Select(f => f.FolloweeId).ToListAsync(ct);
}
