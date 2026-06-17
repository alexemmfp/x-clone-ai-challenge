using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Social.Commands;

public sealed record UnlikeCommand(Guid UserId, Guid TweetId);

public sealed class UnlikeHandler(ILikeRepository likes, IUnitOfWork uow)
{
    public async Task HandleAsync(UnlikeCommand cmd, CancellationToken ct = default)
    {
        var like = await likes.GetAsync(cmd.UserId, cmd.TweetId, ct);
        if (like is null)
        {
            return;
        }

        await likes.RemoveAsync(like, ct);
        await uow.SaveChangesAsync(ct);
    }
}
