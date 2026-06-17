using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Profile.Dtos;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Profile.Queries;

public sealed record GetProfileQuery(string Username, Guid? ViewerId = null);

public sealed class GetProfileHandler(IUserRepository users, IFollowRepository follows)
{
    public async Task<ProfileDto> HandleAsync(GetProfileQuery query, CancellationToken ct = default)
    {
        var user = await users.GetByUsernameAsync(query.Username, ct)
            ?? throw new DomainException("User not found.");

        var followerCount = await follows.CountFollowersAsync(user.Id, ct);
        var followingCount = await follows.CountFollowingAsync(user.Id, ct);

        var isFollowed = query.ViewerId.HasValue
            && await follows.GetAsync(query.ViewerId.Value, user.Id, ct) is not null;

        return new ProfileDto(
            user.Id,
            user.Username,
            user.Email,
            user.Bio,
            user.AvatarUrl,
            followerCount,
            followingCount,
            isFollowed);
    }
}
