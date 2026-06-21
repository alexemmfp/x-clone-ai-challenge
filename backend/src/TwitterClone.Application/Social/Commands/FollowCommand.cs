using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;
using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Application.Social.Commands;

public sealed record FollowCommand(Guid FollowerId, Guid FolloweeId);

public sealed class FollowHandler(
    IFollowRepository follows,
    IUserRepository users,
    IUnitOfWork uow,
    ITimelineNotifier notifier)
{
    public async Task HandleAsync(FollowCommand cmd, CancellationToken ct = default)
    {
        if (cmd.FollowerId == cmd.FolloweeId)
        {
            throw new DomainException("Cannot follow yourself.");
        }

        _ = await users.GetByIdAsync(cmd.FolloweeId, ct)
            ?? throw new DomainException("User not found.");

        var existing = await follows.GetAsync(cmd.FollowerId, cmd.FolloweeId, ct);
        if (existing is not null)
        {
            return;
        }

        await follows.AddAsync(Follow.Create(cmd.FollowerId, cmd.FolloweeId), ct);
        await uow.SaveChangesAsync(ct);

        var follower = await users.GetByIdAsync(cmd.FollowerId, ct);
        await notifier.NotifyFollowedAsync(cmd.FolloweeId,
            follower!.Username, follower.DisplayName, follower.AvatarUrl, ct);
    }
}
