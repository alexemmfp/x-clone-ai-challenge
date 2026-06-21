using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Interfaces;

public interface IFollowRepository
{
    Task<Follow?> GetAsync(Guid followerId, Guid followeeId, CancellationToken ct = default);
    Task AddAsync(Follow follow, CancellationToken ct = default);
    Task RemoveAsync(Follow follow, CancellationToken ct = default);
    Task<int> CountFollowersAsync(Guid userId, CancellationToken ct = default);
    Task<int> CountFollowingAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetFollowerIdsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetFollowingIdsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetFollowerUsersAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetFollowingUsersAsync(Guid userId, CancellationToken ct = default);
}
