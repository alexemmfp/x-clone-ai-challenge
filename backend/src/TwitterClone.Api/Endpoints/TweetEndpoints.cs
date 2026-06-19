using System.Security.Claims;
using FluentValidation;
using TwitterClone.Application.Tweets.Commands;
using TwitterClone.Application.Tweets.Queries;

namespace TwitterClone.Api.Endpoints;

internal static class TweetEndpoints
{
    public static IEndpointRouteBuilder MapTweetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tweets").RequireAuthorization();

        group.MapPost("/", CreateAsync);
        group.MapGet("/{id:guid}", GetTweetAsync);
        group.MapGet("/{id:guid}/replies", GetRepliesAsync);
        group.MapDelete("/{id:guid}", DeleteAsync);
        app.MapGet("/api/timeline", GetTimelineAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateTweetRequest req,
        CreateTweetHandler handler,
        IValidator<CreateTweetCommand> validator,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var cmd = new CreateTweetCommand(userId.Value, req.Text, req.ParentId, req.ImageUrl);
        var result = await validator.ValidateAsync(cmd, ct);
        if (!result.IsValid)
        {
            return Results.ValidationProblem(result.ToDictionary());
        }

        var tweet = await handler.HandleAsync(cmd, ct);
        return Results.Created($"/api/tweets/{tweet.Id}", tweet);
    }

    private static async Task<IResult> GetTweetAsync(
        Guid id,
        GetTweetHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var tweet = await handler.HandleAsync(new GetTweetQuery(userId.Value, id), ct);
        return tweet is null ? Results.NotFound() : Results.Ok(tweet);
    }

    private static async Task<IResult> GetRepliesAsync(
        Guid id,
        GetRepliesHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var replies = await handler.HandleAsync(new GetRepliesQuery(userId.Value, id), ct);
        return Results.Ok(replies);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        DeleteTweetHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        await handler.HandleAsync(new DeleteTweetCommand(id, userId.Value), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetTimelineAsync(
        GetTimelineHandler handler,
        HttpContext ctx,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId(ctx);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var tweets = await handler.HandleAsync(new GetTimelineQuery(userId.Value, page, pageSize), ct);
        return Results.Ok(tweets);
    }

    private static Guid? GetUserId(HttpContext ctx)
    {
        var claim = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? ctx.User.FindFirstValue("sub");
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    private sealed record CreateTweetRequest(string Text, Guid? ParentId = null, string? ImageUrl = null);
}
