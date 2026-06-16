using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Tweets.Dtos;

namespace TwitterClone.Application.Tweets.Queries;

public sealed record GetTimelineQuery(Guid UserId, int Page = 1, int PageSize = 20);

public sealed class GetTimelineHandler(ITweetRepository tweets, IUserRepository users)
{
    public async Task<IReadOnlyList<TweetDto>> HandleAsync(GetTimelineQuery query, CancellationToken ct = default)
    {
        var timeline = await tweets.GetTimelineAsync(query.UserId, query.Page, query.PageSize, ct);

        var authorIds = timeline.Select(t => t.AuthorId).Distinct().ToList();
        var authorMap = new Dictionary<Guid, string>();
        foreach (var id in authorIds)
        {
            var user = await users.GetByIdAsync(id, ct);
            if (user is not null)
            {
                authorMap[id] = user.Username;
            }
        }

        return timeline
            .Select(t => new TweetDto(
                t.Id,
                t.AuthorId,
                authorMap.TryGetValue(t.AuthorId, out var u) ? u : "unknown",
                t.Text,
                t.ParentId,
                t.ImageUrl,
                t.CreatedAt))
            .ToList();
    }
}
