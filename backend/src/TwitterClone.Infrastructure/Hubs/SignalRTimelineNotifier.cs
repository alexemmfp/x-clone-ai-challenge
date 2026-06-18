using Microsoft.AspNetCore.SignalR;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Infrastructure.Hubs;

internal sealed class SignalRTimelineNotifier(IHubContext<TimelineHub> hub) : ITimelineNotifier
{
    public Task NotifyTweetCreatedAsync(TweetDto tweet, CancellationToken ct = default) =>
        hub.Clients.All.SendAsync("TweetCreated", tweet, ct);
}
