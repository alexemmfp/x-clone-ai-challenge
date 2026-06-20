using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Interfaces;

public interface IRetweetRepository
{
    Task<bool> ExistsAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default);
    Task AddAsync(Retweet retweet, CancellationToken ct = default);
    Task RemoveAsync(Guid retweeterId, Guid tweetId, CancellationToken ct = default);
    Task<int> CountAsync(Guid tweetId, CancellationToken ct = default);
    Task<IReadOnlyList<(Tweet Tweet, string RetweeterUsername, DateTime RetweetedAt)>>
        GetTimelineRetweetsAsync(Guid viewerId, int count, CancellationToken ct = default);
}
