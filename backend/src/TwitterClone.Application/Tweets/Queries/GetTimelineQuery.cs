using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Application.Tweets.Queries;

public sealed record GetTimelineQuery(Guid UserId, int Page = 1, int PageSize = 20);

public sealed class GetTimelineHandler(
    ITweetRepository tweets,
    IUserRepository users,
    ILikeRepository likes,
    IRetweetRepository retweets)
{
    public async Task<IReadOnlyList<TweetDto>> HandleAsync(GetTimelineQuery query, CancellationToken ct = default)
    {
        var fetchCount = query.PageSize * 2;

        var tweetEntities = await tweets.GetTimelineAsync(query.UserId, query.Page, fetchCount, ct);
        var retweetEntries = await retweets.GetTimelineRetweetsAsync(query.UserId, query.Page, fetchCount, ct);

        var allTweetIds = tweetEntities.Select(t => t.Id)
            .Concat(retweetEntries.Select(r => r.Tweet.Id))
            .Distinct()
            .ToList();

        var authorIds = tweetEntities.Select(t => t.AuthorId)
            .Concat(retweetEntries.Select(r => r.Tweet.AuthorId))
            .Distinct()
            .ToList();

        var authorMap = await users.GetByIdsAsync(authorIds, ct);
        var likeCounts = await likes.CountForTweetsAsync(allTweetIds, ct);
        var likedByViewer = await likes.GetLikedByUserAsync(query.UserId, allTweetIds, ct);
        var retweetCounts = await retweets.CountForTweetsAsync(allTweetIds, ct);
        var retweetedByViewer = await retweets.GetRetweetedByUserAsync(query.UserId, allTweetIds, ct);
        var replyCounts = await tweets.GetReplyCountsAsync(allTweetIds, ct);

        var result = new List<(TweetDto Dto, DateTime DisplayAt)>();

        foreach (var t in tweetEntities)
        {
            var author = authorMap.TryGetValue(t.AuthorId, out var a) ? a : null;
            result.Add((new TweetDto(
                t.Id, t.AuthorId, author?.Username ?? "unknown",
                t.Text, t.ParentId, t.ImageUrl, t.CreatedAt,
                likeCounts.GetValueOrDefault(t.Id),
                likedByViewer.Contains(t.Id),
                replyCounts.GetValueOrDefault(t.Id),
                retweetCounts.GetValueOrDefault(t.Id),
                retweetedByViewer.Contains(t.Id),
                AuthorDisplayName: author?.DisplayName,
                AuthorAvatarUrl: author?.AvatarUrl), t.CreatedAt));
        }

        foreach (var (tweet, retweeterUsername, retweetedAt) in retweetEntries)
        {
            var author = authorMap.TryGetValue(tweet.AuthorId, out var a) ? a : null;
            result.Add((new TweetDto(
                tweet.Id, tweet.AuthorId, author?.Username ?? "unknown",
                tweet.Text, tweet.ParentId, tweet.ImageUrl, tweet.CreatedAt,
                likeCounts.GetValueOrDefault(tweet.Id),
                likedByViewer.Contains(tweet.Id),
                replyCounts.GetValueOrDefault(tweet.Id),
                retweetCounts.GetValueOrDefault(tweet.Id),
                retweetedByViewer.Contains(tweet.Id),
                IsRetweet: true, RetweetedByUsername: retweeterUsername,
                AuthorDisplayName: author?.DisplayName,
                AuthorAvatarUrl: author?.AvatarUrl), retweetedAt));
        }

        return result
            .OrderByDescending(x => x.DisplayAt)
            .DistinctBy(x => (x.Dto.Id, x.Dto.IsRetweet ? x.Dto.RetweetedByUsername : null))
            .Take(query.PageSize)
            .Select(x => x.Dto)
            .ToList();
    }
}
