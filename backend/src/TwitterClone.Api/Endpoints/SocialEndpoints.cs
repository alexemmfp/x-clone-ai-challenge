using System.Security.Claims;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Social.Commands;
using TwitterClone.Application.Tweets.Commands;

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
        group.MapPost("/tweets/{id:guid}/retweet", RetweetAsync);
        group.MapDelete("/tweets/{id:guid}/retweet", UnretweetAsync);

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

    private static async Task<IResult> RetweetAsync(
        Guid id,
        RetweetHandler handler,
        IRetweetRepository retweetRepo,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        try
        {
            await handler.HandleAsync(new RetweetCommand(userId.Value, id), ct);
        }
        catch (TwitterClone.Domain.Exceptions.DomainException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }

        var count = await retweetRepo.CountAsync(id, ct);
        return Results.Ok(new { retweetCount = count });
    }

    private static async Task<IResult> UnretweetAsync(
        Guid id,
        UnretweetHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        await handler.HandleAsync(new UnretweetCommand(userId.Value, id), ct);
        return Results.NoContent();
    }

    private static Guid? GetUserId(HttpContext ctx)
    {
        var claim = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? ctx.User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
