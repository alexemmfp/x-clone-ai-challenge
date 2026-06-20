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

        var tweetEntities = await tweets.GetTimelineAsync(query.UserId, 1, fetchCount, ct);
        var retweetEntries = await retweets.GetTimelineRetweetsAsync(query.UserId, fetchCount, ct);

        var authorIds = tweetEntities.Select(t => t.AuthorId)
            .Concat(retweetEntries.Select(r => r.Tweet.AuthorId))
            .Distinct()
            .ToList();

        var authorMap = new Dictionary<Guid, string>();
        foreach (var id in authorIds)
        {
            var user = await users.GetByIdAsync(id, ct);
            if (user is not null) { authorMap[id] = user.Username; }
        }

        var result = new List<(TweetDto Dto, DateTime DisplayAt)>();

        foreach (var t in tweetEntities)
        {
            var likeCount = await likes.CountAsync(t.Id, ct);
            var likedByViewer = await likes.GetAsync(query.UserId, t.Id, ct) is not null;
            var retweetCount = await retweets.CountAsync(t.Id, ct);
            var retweetedByViewer = await retweets.ExistsAsync(query.UserId, t.Id, ct);
            result.Add((new TweetDto(
                t.Id, t.AuthorId,
                authorMap.TryGetValue(t.AuthorId, out var u) ? u : "unknown",
                t.Text, t.ParentId, t.ImageUrl, t.CreatedAt,
                likeCount, likedByViewer, retweetCount, retweetedByViewer), t.CreatedAt));
        }

        foreach (var (tweet, retweeterUsername, retweetedAt) in retweetEntries)
        {
            var likeCount = await likes.CountAsync(tweet.Id, ct);
            var likedByViewer = await likes.GetAsync(query.UserId, tweet.Id, ct) is not null;
            var retweetCount = await retweets.CountAsync(tweet.Id, ct);
            var retweetedByViewer = await retweets.ExistsAsync(query.UserId, tweet.Id, ct);
            result.Add((new TweetDto(
                tweet.Id, tweet.AuthorId,
                authorMap.TryGetValue(tweet.AuthorId, out var u2) ? u2 : "unknown",
                tweet.Text, tweet.ParentId, tweet.ImageUrl, tweet.CreatedAt,
                likeCount, likedByViewer, retweetCount, retweetedByViewer,
                IsRetweet: true, RetweetedByUsername: retweeterUsername), retweetedAt));
        }

        return result
            .OrderByDescending(x => x.DisplayAt)
            .DistinctBy(x => (x.Dto.Id, x.Dto.IsRetweet ? x.Dto.RetweetedByUsername : null))
            .Take(query.PageSize)
            .Select(x => x.Dto)
            .ToList();
    }
}
