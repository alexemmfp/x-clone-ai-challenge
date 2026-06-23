using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Application.Interfaces;

public interface ITimelineNotifier
{
    Task NotifyTweetCreatedAsync(TweetDto tweet, CancellationToken ct = default);
    Task NotifyFollowedAsync(Guid targetUserId, string followerUsername, string? displayName, string? avatarUrl, CancellationToken ct = default);
    Task NotifyMentionedAsync(Guid targetUserId, Guid tweetId, string authorUsername, string tweetText, CancellationToken ct = default);
    Task NotifyRetweetedAsync(Guid targetUserId, Guid tweetId, string retweeterUsername, CancellationToken ct = default);
    Task NotifyLikedAsync(Guid targetUserId, Guid tweetId, string likerUsername, CancellationToken ct = default);
    Task NotifyRepliedAsync(Guid targetUserId, Guid tweetId, string replierUsername, string replyText, CancellationToken ct = default);
}
