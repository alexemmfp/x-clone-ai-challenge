using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Application.Interfaces;

public interface ITimelineNotifier
{
    Task NotifyTweetCreatedAsync(TweetDto tweet, CancellationToken ct = default);
}
