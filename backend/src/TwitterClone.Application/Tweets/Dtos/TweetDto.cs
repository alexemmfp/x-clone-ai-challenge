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
    bool LikedByViewer = false,
    int ReplyCount = 0,
    int RetweetCount = 0,
    bool RetweetedByViewer = false,
    bool IsRetweet = false,
    string? RetweetedByUsername = null,
    string? AuthorDisplayName = null,
    string? AuthorAvatarUrl = null);
