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

        var authorIds = tweetEntities.Select(t => t.AuthorId)
            .Concat(retweetEntries.Select(r => r.Tweet.AuthorId))
            .Distinct()
            .ToList();

        var authorMap = new Dictionary<Guid, (string Username, string? DisplayName, string? AvatarUrl)>();
        foreach (var id in authorIds)
        {
            var user = await users.GetByIdAsync(id, ct);
            if (user is not null) { authorMap[id] = (user.Username, user.DisplayName, user.AvatarUrl); }
        }

        var result = new List<(TweetDto Dto, DateTime DisplayAt)>();

        foreach (var t in tweetEntities)
        {
            var likeCount = await likes.CountAsync(t.Id, ct);
            var likedByViewer = await likes.GetAsync(query.UserId, t.Id, ct) is not null;
            var replyCount = await tweets.GetReplyCountAsync(t.Id, ct);
            var retweetCount = await retweets.CountAsync(t.Id, ct);
            var retweetedByViewer = await retweets.ExistsAsync(query.UserId, t.Id, ct);
            var (username, displayName, avatarUrl) = authorMap.TryGetValue(t.AuthorId, out var info)
                ? info : ("unknown", null, null);
            result.Add((new TweetDto(
                t.Id, t.AuthorId, username,
                t.Text, t.ParentId, t.ImageUrl, t.CreatedAt,
                likeCount, likedByViewer, replyCount, retweetCount, retweetedByViewer,
                AuthorDisplayName: displayName, AuthorAvatarUrl: avatarUrl), t.CreatedAt));
        }

        foreach (var (tweet, retweeterUsername, retweetedAt) in retweetEntries)
        {
            var likeCount = await likes.CountAsync(tweet.Id, ct);
            var likedByViewer = await likes.GetAsync(query.UserId, tweet.Id, ct) is not null;
            var replyCount = await tweets.GetReplyCountAsync(tweet.Id, ct);
            var retweetCount = await retweets.CountAsync(tweet.Id, ct);
            var retweetedByViewer = await retweets.ExistsAsync(query.UserId, tweet.Id, ct);
            var (username2, displayName2, avatarUrl2) = authorMap.TryGetValue(tweet.AuthorId, out var info2)
                ? info2 : ("unknown", null, null);
            result.Add((new TweetDto(
                tweet.Id, tweet.AuthorId, username2,
                tweet.Text, tweet.ParentId, tweet.ImageUrl, tweet.CreatedAt,
                likeCount, likedByViewer, replyCount, retweetCount, retweetedByViewer,
                IsRetweet: true, RetweetedByUsername: retweeterUsername,
                AuthorDisplayName: displayName2, AuthorAvatarUrl: avatarUrl2), retweetedAt));
        }

        return result
            .OrderByDescending(x => x.DisplayAt)
            .DistinctBy(x => (x.Dto.Id, x.Dto.IsRetweet ? x.Dto.RetweetedByUsername : null))
            .Take(query.PageSize)
            .Select(x => x.Dto)
            .ToList();
    }
}
