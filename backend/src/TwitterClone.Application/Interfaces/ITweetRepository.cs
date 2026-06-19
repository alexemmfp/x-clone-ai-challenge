using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Interfaces;

public interface ITweetRepository
{
    Task<Tweet?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Tweet tweet, CancellationToken ct = default);
    Task RemoveAsync(Tweet tweet, CancellationToken ct = default);
    Task<IReadOnlyList<Tweet>> GetTimelineAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Tweet>> GetRepliesAsync(Guid parentId, CancellationToken ct = default);
}
