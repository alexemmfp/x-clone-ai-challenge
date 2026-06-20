using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Application.Tweets.Queries;

public sealed record GetTweetQuery(Guid ViewerId, Guid TweetId);

public sealed class GetTweetHandler(ITweetRepository tweets, IUserRepository users, ILikeRepository likes)
{
    public async Task<TweetDto?> HandleAsync(GetTweetQuery query, CancellationToken ct = default)
    {
        var tweet = await tweets.GetByIdAsync(query.TweetId, ct);
        if (tweet is null)
        {
            return null;
        }

        var author = await users.GetByIdAsync(tweet.AuthorId, ct);
        var likeCount = await likes.CountAsync(tweet.Id, ct);
        var likedByViewer = await likes.GetAsync(query.ViewerId, tweet.Id, ct) is not null;
        var replyCount = await tweets.GetReplyCountAsync(tweet.Id, ct);

        return new TweetDto(
            tweet.Id,
            tweet.AuthorId,
            author?.Username ?? "unknown",
            tweet.Text,
            tweet.ParentId,
            tweet.ImageUrl,
            tweet.CreatedAt,
            likeCount,
            likedByViewer,
            replyCount,
            AuthorDisplayName: author?.DisplayName,
            AuthorAvatarUrl: author?.AvatarUrl);
    }
}
