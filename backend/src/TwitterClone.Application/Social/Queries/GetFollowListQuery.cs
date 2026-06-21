using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Social.Dtos;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Social.Queries;

public sealed record GetFollowListQuery(string Username, bool Followers);

public sealed class GetFollowListHandler(IUserRepository users, IFollowRepository follows)
{
    public async Task<IReadOnlyList<UserSummaryDto>> HandleAsync(GetFollowListQuery query, CancellationToken ct = default)
    {
        var user = await users.GetByUsernameAsync(query.Username, ct)
            ?? throw new DomainException("User not found.");

        var list = query.Followers
            ? await follows.GetFollowerUsersAsync(user.Id, ct)
            : await follows.GetFollowingUsersAsync(user.Id, ct);

        return list.Select(u => new UserSummaryDto(u.Id, u.Username, u.DisplayName, u.AvatarUrl)).ToList();
    }
}
