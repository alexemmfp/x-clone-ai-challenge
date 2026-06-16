using System.Security.Claims;
using FluentValidation;
using TwitterClone.Application.Auth.Commands;

namespace TwitterClone.Api.Endpoints;

internal static class AuthEndpoints
{
    private const string RefreshTokenCookie = "refresh_token";
    private static readonly CookieOptions CookieOpts = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/api/auth",
    };

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/refresh", RefreshAsync);
        group.MapPost("/logout", LogoutAsync).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterCommand cmd,
        RegisterHandler handler,
        IValidator<RegisterCommand> validator,
        HttpContext ctx,
        CancellationToken ct)
    {
        var result = await validator.ValidateAsync(cmd, ct);
        if (!result.IsValid)
        {
            return Results.ValidationProblem(result.ToDictionary());
        }

        var auth = await handler.HandleAsync(cmd, ct);
        ctx.Response.Cookies.Append(RefreshTokenCookie, auth.RefreshToken, CookieOpts);
        return Results.Ok(new { auth.AccessToken, auth.UserId, auth.Username });
    }

    private static async Task<IResult> LoginAsync(
        LoginCommand cmd,
        LoginHandler handler,
        IValidator<LoginCommand> validator,
        HttpContext ctx,
        CancellationToken ct)
    {
        var result = await validator.ValidateAsync(cmd, ct);
        if (!result.IsValid)
        {
            return Results.ValidationProblem(result.ToDictionary());
        }

        var auth = await handler.HandleAsync(cmd, ct);
        ctx.Response.Cookies.Append(RefreshTokenCookie, auth.RefreshToken, CookieOpts);
        return Results.Ok(new { auth.AccessToken, auth.UserId, auth.Username });
    }

    private static async Task<IResult> RefreshAsync(
        RefreshHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var token = ctx.Request.Cookies[RefreshTokenCookie];
        if (string.IsNullOrEmpty(token))
        {
            return Results.Unauthorized();
        }

        var auth = await handler.HandleAsync(new RefreshCommand(token), ct);
        ctx.Response.Cookies.Append(RefreshTokenCookie, auth.RefreshToken, CookieOpts);
        return Results.Ok(new { auth.AccessToken, auth.UserId, auth.Username });
    }

    private static async Task<IResult> LogoutAsync(
        LogoutHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? ctx.User.FindFirstValue("sub");

        if (!Guid.TryParse(userId, out var id))
        {
            return Results.Unauthorized();
        }

        await handler.HandleAsync(new LogoutCommand(id), ct);
        ctx.Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = "/api/auth" });
        return Results.NoContent();
    }
}
