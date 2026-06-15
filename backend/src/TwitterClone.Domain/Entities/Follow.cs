namespace TwitterClone.Domain.Entities;

public class Follow
{
    public Guid FollowerId { get; private set; }
    public Guid FolloweeId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Follow() { }

    public static Follow Create(Guid followerId, Guid followeeId) =>
        new() { FollowerId = followerId, FolloweeId = followeeId, CreatedAt = DateTime.UtcNow };
}
