using TwitterClone.Application.Interfaces;

namespace TwitterClone.Infrastructure.Storage;

internal sealed class LocalFileStorageService(string uploadsPath) : IFileStorageService
{
    private static readonly Dictionary<string, string> ContentTypeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/gif"] = ".gif",
        ["image/webp"] = ".webp",
    };

    public async Task<string> SaveAsync(Stream fileStream, string contentType, CancellationToken ct = default)
    {
        Directory.CreateDirectory(uploadsPath);

        var ext = ContentTypeExtensions.TryGetValue(contentType, out var mapped) ? mapped : ".bin";
        var stored = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsPath, stored);

        await using var dest = File.Create(fullPath);
        await fileStream.CopyToAsync(dest, ct);

        return $"/uploads/{stored}";
    }
}
