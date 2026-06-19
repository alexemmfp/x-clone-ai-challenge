using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Application.Tweets.Queries;

public sealed record GetRepliesQuery(Guid ViewerId, Guid ParentId);

public sealed class GetRepliesHandler(ITweetRepository tweets, IUserRepository users, ILikeRepository likes)
{
    public async Task<IReadOnlyList<TweetDto>> HandleAsync(GetRepliesQuery query, CancellationToken ct = default)
    {
        var replies = await tweets.GetRepliesAsync(query.ParentId, ct);

        var authorIds = replies.Select(t => t.AuthorId).Distinct().ToList();
        var authorMap = new Dictionary<Guid, string>();
        foreach (var id in authorIds)
        {
            var user = await users.GetByIdAsync(id, ct);
            if (user is not null)
            {
                authorMap[id] = user.Username;
            }
        }

        var result = new List<TweetDto>();
        foreach (var t in replies)
        {
            var likeCount = await likes.CountAsync(t.Id, ct);
            var likedByViewer = await likes.GetAsync(query.ViewerId, t.Id, ct) is not null;
            result.Add(new TweetDto(
                t.Id,
                t.AuthorId,
                authorMap.TryGetValue(t.AuthorId, out var u) ? u : "unknown",
                t.Text,
                t.ParentId,
                t.ImageUrl,
                t.CreatedAt,
                likeCount,
                likedByViewer));
        }

        return result;
    }
}
