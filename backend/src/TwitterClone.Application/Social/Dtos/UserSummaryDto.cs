namespace TwitterClone.Application.Social.Dtos;

public sealed record UserSummaryDto(Guid Id, string Username, string? DisplayName, string? AvatarUrl);
