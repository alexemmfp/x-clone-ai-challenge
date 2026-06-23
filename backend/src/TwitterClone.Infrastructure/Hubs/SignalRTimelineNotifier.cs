using Microsoft.AspNetCore.SignalR;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Infrastructure.Hubs;

internal sealed class SignalRTimelineNotifier(IHubContext<TimelineHub> hub) : ITimelineNotifier
{
    public Task NotifyTweetCreatedAsync(TweetDto tweet, CancellationToken ct = default) =>
        hub.Clients.All.SendAsync("TweetCreated", tweet, ct);

    public Task NotifyFollowedAsync(Guid targetUserId, string followerUsername, string? displayName, string? avatarUrl, CancellationToken ct = default) =>
        hub.Clients.Group($"user-{targetUserId}").SendAsync("FollowNotification",
            new { followerUsername, displayName, avatarUrl }, ct);

    public Task NotifyMentionedAsync(Guid targetUserId, Guid tweetId, string authorUsername, string tweetText, CancellationToken ct = default) =>
        hub.Clients.Group($"user-{targetUserId}").SendAsync("MentionNotification",
            new { tweetId, authorUsername, tweetText }, ct);

    public Task NotifyRetweetedAsync(Guid targetUserId, Guid tweetId, string retweeterUsername, CancellationToken ct = default) =>
        hub.Clients.Group($"user-{targetUserId}").SendAsync("RetweetNotification",
            new { tweetId, retweeterUsername }, ct);

    public Task NotifyLikedAsync(Guid targetUserId, Guid tweetId, string likerUsername, CancellationToken ct = default) =>
        hub.Clients.Group($"user-{targetUserId}").SendAsync("LikeNotification",
            new { tweetId, likerUsername }, ct);

    public Task NotifyRepliedAsync(Guid targetUserId, Guid tweetId, string replierUsername, string replyText, CancellationToken ct = default) =>
        hub.Clients.Group($"user-{targetUserId}").SendAsync("ReplyNotification",
            new { tweetId, replierUsername, replyText }, ct);
}
