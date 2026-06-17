namespace TwitterClone.Application.Tweets.Dtos;

public sealed record TweetDto(
    Guid Id,
    Guid AuthorId,
    string AuthorUsername,
    string Text,
    Guid? ParentId,
    string? ImageUrl,
    DateTime CreatedAt,
    int LikeCount = 0,
    bool LikedByViewer = false);
