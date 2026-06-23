using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Application.Tweets.Queries;

public sealed record GetRepliesQuery(Guid ViewerId, Guid ParentId);

public sealed class GetRepliesHandler(ITweetRepository tweets, IUserRepository users, ILikeRepository likes)
{
    public async Task<IReadOnlyList<TweetDto>> HandleAsync(GetRepliesQuery query, CancellationToken ct = default)
    {
        var replies = await tweets.GetRepliesAsync(query.ParentId, ct);
        if (replies.Count == 0) { return []; }

        var tweetIds = replies.Select(t => t.Id).ToList();
        var authorIds = replies.Select(t => t.AuthorId).Distinct().ToList();

        var authorMap = await users.GetByIdsAsync(authorIds, ct);
        var likeCounts = await likes.CountForTweetsAsync(tweetIds, ct);
        var likedByViewer = await likes.GetLikedByUserAsync(query.ViewerId, tweetIds, ct);
        var replyCounts = await tweets.GetReplyCountsAsync(tweetIds, ct);

        var result = new List<TweetDto>(replies.Count);
        foreach (var t in replies)
        {
            var author = authorMap.TryGetValue(t.AuthorId, out var a) ? a : null;
            result.Add(new TweetDto(
                t.Id,
                t.AuthorId,
                author?.Username ?? "unknown",
                t.Text,
                t.ParentId,
                t.ImageUrl,
                t.CreatedAt,
                likeCounts.GetValueOrDefault(t.Id),
                likedByViewer.Contains(t.Id),
                replyCounts.GetValueOrDefault(t.Id),
                AuthorDisplayName: author?.DisplayName,
                AuthorAvatarUrl: author?.AvatarUrl));
        }

        return result;
    }
}
