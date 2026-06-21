using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TwitterClone.Infrastructure.Hubs;

[Authorize]
public sealed class TimelineHub : Hub
{
    public override Task OnConnectedAsync()
    {
        if (Context.UserIdentifier is not null)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"user-{Context.UserIdentifier}");
        }

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.UserIdentifier is not null)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{Context.UserIdentifier}");
        }

        return base.OnDisconnectedAsync(exception);
    }
}
