using System.Security.Claims;
using FluentValidation;
using TwitterClone.Application.Profile.Commands;
using TwitterClone.Application.Profile.Queries;
using TwitterClone.Application.Search.Queries;
using TwitterClone.Application.Social.Queries;

namespace TwitterClone.Api.Endpoints;

internal static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{username}", GetProfileAsync);
        app.MapGet("/api/users/{username}/followers", GetFollowersAsync);
        app.MapGet("/api/users/{username}/following", GetFollowingAsync);
        app.MapGet("/api/search/users", SearchUsersAsync).RequireAuthorization();
        app.MapPatch("/api/me", UpdateProfileAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> GetProfileAsync(
        string username,
        GetProfileHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var viewerClaim = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? ctx.User.FindFirstValue("sub");
        Guid? viewerId = Guid.TryParse(viewerClaim, out var parsed) ? parsed : null;

        var profile = await handler.HandleAsync(new GetProfileQuery(username, viewerId), ct);

        return Results.Ok(profile);
    }

    private static async Task<IResult> UpdateProfileAsync(
        UpdateProfileRequest req,
        UpdateProfileHandler handler,
        IValidator<UpdateProfileCommand> validator,
        HttpContext ctx,
        CancellationToken ct)
    {
        var claim = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? ctx.User.FindFirstValue("sub");
        if (!Guid.TryParse(claim, out var userId))
        {
            return Results.Unauthorized();
        }

        var cmd = new UpdateProfileCommand(userId, req.Bio, req.AvatarUrl, req.DisplayName);
        var result = await validator.ValidateAsync(cmd, ct);
        if (!result.IsValid)
        {
            return Results.ValidationProblem(result.ToDictionary());
        }

        var profile = await handler.HandleAsync(cmd, ct);
        return Results.Ok(profile);
    }

    private static async Task<IResult> SearchUsersAsync(
        string q,
        SearchUsersHandler handler,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.Ok(Array.Empty<object>());
        }

        var results = await handler.HandleAsync(new SearchUsersQuery(q), ct);
        return Results.Ok(results);
    }

    private static async Task<IResult> GetFollowersAsync(
        string username,
        GetFollowListHandler handler,
        CancellationToken ct)
    {
        var list = await handler.HandleAsync(new GetFollowListQuery(username, Followers: true), ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetFollowingAsync(
        string username,
        GetFollowListHandler handler,
        CancellationToken ct)
    {
        var list = await handler.HandleAsync(new GetFollowListQuery(username, Followers: false), ct);
        return Results.Ok(list);
    }

    private sealed record UpdateProfileRequest(string? Bio, string? AvatarUrl, string? DisplayName);
}
