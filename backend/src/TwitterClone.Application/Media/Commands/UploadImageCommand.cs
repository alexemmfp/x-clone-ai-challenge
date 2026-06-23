using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Media.Commands;

public sealed record UploadImageCommand(Stream FileStream, string FileName, string ContentType);

public sealed class UploadImageHandler(IFileStorageService storage)
{
    private const long MaxBytes = 5 * 1024 * 1024;

    public async Task<string> HandleAsync(UploadImageCommand cmd, CancellationToken ct = default)
    {
        if (cmd.FileStream.Length > MaxBytes)
        {
            throw new InvalidOperationException("File exceeds maximum size of 5 MB.");
        }

        return await storage.SaveAsync(cmd.FileStream, cmd.ContentType, ct);
    }
}
