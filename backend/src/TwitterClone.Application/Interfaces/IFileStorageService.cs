namespace TwitterClone.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string contentType, CancellationToken ct = default);
}
