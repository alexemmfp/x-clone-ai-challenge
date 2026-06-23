using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<IReadOnlyList<User>> SearchAsync(string term, int limit, CancellationToken ct = default);
    Task<IReadOnlySet<string>> GetExistingUsernamesAsync(IEnumerable<string> usernames, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, User>> GetByUsernamesAsync(IEnumerable<string> usernames, CancellationToken ct = default);
}
