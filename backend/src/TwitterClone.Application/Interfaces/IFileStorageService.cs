namespace TwitterClone.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}
