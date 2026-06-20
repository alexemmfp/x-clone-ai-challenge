using TwitterClone.Application.Users.Queries;

namespace TwitterClone.Api.Endpoints;

internal static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/validate-usernames", ValidateUsernamesAsync);
        return app;
    }

    private static async Task<IResult> ValidateUsernamesAsync(
        string? usernames,
        ValidateUsernamesHandler handler,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(usernames))
        {
            return Results.Ok(new Dictionary<string, bool>());
        }

        var parsed = usernames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = await handler.HandleAsync(new ValidateUsernamesQuery(parsed), ct);
        return Results.Ok(result);
    }
}
