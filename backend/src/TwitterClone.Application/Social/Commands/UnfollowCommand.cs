using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Social.Commands;

public sealed record UnfollowCommand(Guid FollowerId, Guid FolloweeId);

public sealed class UnfollowHandler(IFollowRepository follows, IUnitOfWork uow)
{
    public async Task HandleAsync(UnfollowCommand cmd, CancellationToken ct = default)
    {
        var follow = await follows.GetAsync(cmd.FollowerId, cmd.FolloweeId, ct);
        if (follow is null)
        {
            return;
        }

        await follows.RemoveAsync(follow, ct);
        await uow.SaveChangesAsync(ct);
    }
}
