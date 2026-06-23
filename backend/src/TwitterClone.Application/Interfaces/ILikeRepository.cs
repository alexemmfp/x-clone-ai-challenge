using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Interfaces;

#pragma warning disable CA1716
public interface ILikeRepository
{
    Task<Like?> GetAsync(Guid userId, Guid tweetId, CancellationToken ct = default);
    Task AddAsync(Like like, CancellationToken ct = default);
    Task RemoveAsync(Like like, CancellationToken ct = default);
    Task<int> CountAsync(Guid tweetId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, int>> CountForTweetsAsync(IEnumerable<Guid> tweetIds, CancellationToken ct = default);
    Task<IReadOnlySet<Guid>> GetLikedByUserAsync(Guid userId, IEnumerable<Guid> tweetIds, CancellationToken ct = default);
}
#pragma warning restore CA1716
