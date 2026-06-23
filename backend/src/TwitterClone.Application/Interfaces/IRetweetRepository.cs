using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Interfaces;

public interface IRetweetRepository
{
    Task<bool> ExistsAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default);
    Task AddAsync(Retweet retweet, CancellationToken ct = default);
    Task RemoveAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default);
    Task<int> CountAsync(Guid tweetId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, int>> CountForTweetsAsync(IEnumerable<Guid> tweetIds, CancellationToken ct = default);
    Task<IReadOnlySet<Guid>> GetRetweetedByUserAsync(Guid userId, IEnumerable<Guid> tweetIds, CancellationToken ct = default);
    Task<IReadOnlyList<(Tweet Tweet, string RetweeterUsername, DateTime RetweetedAt)>>
        GetTimelineRetweetsAsync(Guid viewerId, int page, int count, CancellationToken ct = default);
}
