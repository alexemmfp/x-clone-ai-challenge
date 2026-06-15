#pragma warning disable CA1716 // "Like" conflicts with VB keyword — domain name is intentional
namespace TwitterClone.Domain.Entities;

public class Like
{
    public Guid UserId { get; private set; }
    public Guid TweetId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Like() { }

    public static Like Create(Guid userId, Guid tweetId) =>
        new() { UserId = userId, TweetId = tweetId, CreatedAt = DateTime.UtcNow };
}
#pragma warning restore CA1716
