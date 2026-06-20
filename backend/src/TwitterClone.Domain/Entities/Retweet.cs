using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Entities;

public sealed class Retweet
{
    public Guid Id { get; private set; }
    public Guid RetweeterId { get; private set; }
    public Guid TweetId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Retweet() { }

    public static Retweet Create(Guid retweeterId, Guid tweetId, Guid authorId)
    {
        if (retweeterId == authorId)
        {
            throw new DomainException("Cannot retweet own tweet.");
        }

        return new Retweet
        {
            Id = Guid.NewGuid(),
            RetweeterId = retweeterId,
            TweetId = tweetId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
