using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Profile.Dtos;

namespace TwitterClone.Application.Search.Queries;

public sealed record SearchUsersQuery(string Term, int Limit = 10);

public sealed class SearchUsersHandler(IUserRepository users)
{
    public async Task<IReadOnlyList<ProfileDto>> HandleAsync(SearchUsersQuery query, CancellationToken ct = default)
    {
        var results = await users.SearchAsync(query.Term, query.Limit, ct);
        return results
            .Select(u => new ProfileDto(u.Id, u.Username, u.Email, u.Bio, u.AvatarUrl, 0, 0, false))
            .ToList();
    }
}
