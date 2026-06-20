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
        var authorMap = new Dictionary<Guid, (string Username, string? DisplayName, string? AvatarUrl)>();
        foreach (var id in authorIds)
        {
            var user = await users.GetByIdAsync(id, ct);
            if (user is not null)
            {
                authorMap[id] = (user.Username, user.DisplayName, user.AvatarUrl);
            }
        }

        var result = new List<TweetDto>();
        foreach (var t in replies)
        {
            var likeCount = await likes.CountAsync(t.Id, ct);
            var likedByViewer = await likes.GetAsync(query.ViewerId, t.Id, ct) is not null;
            var replyCount = await tweets.GetReplyCountAsync(t.Id, ct);
            var (username, displayName, avatarUrl) = authorMap.TryGetValue(t.AuthorId, out var info)
                ? info : ("unknown", null, null);
            result.Add(new TweetDto(
                t.Id,
                t.AuthorId,
                username,
                t.Text,
                t.ParentId,
                t.ImageUrl,
                t.CreatedAt,
                likeCount,
                likedByViewer,
                replyCount,
                AuthorDisplayName: displayName,
                AuthorAvatarUrl: avatarUrl));
        }

        return result;
    }
}
