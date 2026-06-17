namespace TwitterClone.Application.Profile.Dtos;

public sealed record ProfileDto(
    Guid Id,
    string Username,
    string Email,
    string? Bio,
    string? AvatarUrl,
    int FollowerCount,
    int FollowingCount,
    bool IsFollowedByViewer);
