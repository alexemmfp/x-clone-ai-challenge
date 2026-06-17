using System.Security.Claims;
using TwitterClone.Application.Social.Commands;

namespace TwitterClone.Api.Endpoints;

internal static class SocialEndpoints
{
    public static IEndpointRouteBuilder MapSocialEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api").RequireAuthorization();

        group.MapPost("/users/{username}/follow", FollowAsync);
        group.MapDelete("/users/{username}/follow", UnfollowAsync);
        group.MapPost("/tweets/{id:guid}/like", LikeAsync);
        group.MapDelete("/tweets/{id:guid}/like", UnlikeAsync);

        return app;
    }

    private static async Task<IResult> FollowAsync(
        string username,
        FollowHandler handler,
        IServiceProvider sp,
        HttpContext ctx,
        CancellationToken ct)
    {
        var followerId = GetUserId(ctx);
        if (followerId is null)
        {
            return Results.Unauthorized();
        }

        var users = sp.GetRequiredService<TwitterClone.Application.Interfaces.IUserRepository>();
        var followee = await users.GetByUsernameAsync(username, ct);
        if (followee is null)
        {
            return Results.NotFound();
        }

        await handler.HandleAsync(new FollowCommand(followerId.Value, followee.Id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UnfollowAsync(
        string username,
        UnfollowHandler handler,
        IServiceProvider sp,
        HttpContext ctx,
        CancellationToken ct)
    {
        var followerId = GetUserId(ctx);
        if (followerId is null)
        {
            return Results.Unauthorized();
        }

        var users = sp.GetRequiredService<TwitterClone.Application.Interfaces.IUserRepository>();
        var followee = await users.GetByUsernameAsync(username, ct);
        if (followee is null)
        {
            return Results.NotFound();
        }

        await handler.HandleAsync(new UnfollowCommand(followerId.Value, followee.Id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> LikeAsync(
        Guid id,
        LikeHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        await handler.HandleAsync(new LikeCommand(userId.Value, id), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> UnlikeAsync(
        Guid id,
        UnlikeHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        await handler.HandleAsync(new UnlikeCommand(userId.Value, id), ct);
        return Results.NoContent();
    }

    private static Guid? GetUserId(HttpContext ctx)
    {
        var claim = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? ctx.User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
