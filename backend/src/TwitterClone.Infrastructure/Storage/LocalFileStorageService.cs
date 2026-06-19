using TwitterClone.Application.Interfaces;

namespace TwitterClone.Infrastructure.Storage;

internal sealed class LocalFileStorageService(string uploadsPath) : IFileStorageService
{
    public async Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        Directory.CreateDirectory(uploadsPath);

        var ext = Path.GetExtension(fileName);
        var stored = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(uploadsPath, stored);

        await using var dest = File.Create(fullPath);
        await fileStream.CopyToAsync(dest, ct);

        return $"/uploads/{stored}";
    }
}
