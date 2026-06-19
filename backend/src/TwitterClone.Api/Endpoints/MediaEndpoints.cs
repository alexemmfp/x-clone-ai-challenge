using TwitterClone.Application.Media.Commands;

namespace TwitterClone.Api.Endpoints;

internal static class MediaEndpoints
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp"
    ];

    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/media/upload", UploadAsync).RequireAuthorization().DisableAntiforgery();
        return app;
    }

    private static async Task<IResult> UploadAsync(
        IFormFile? file,
        UploadImageHandler handler,
        HttpContext ctx,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided." });
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return Results.BadRequest(new { error = "Unsupported image type." });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var url = await handler.HandleAsync(new UploadImageCommand(stream, file.FileName, file.ContentType), ct);
            return Results.Ok(new { url });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
