using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();
        return db.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToLowerInvariant();
        return db.Users.AnyAsync(u => u.Email == normalized, ct);
    }

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default) =>
        db.Users.AnyAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);

    public async Task<IReadOnlyList<User>> SearchAsync(string term, int limit, CancellationToken ct = default)
    {
        var lower = term.ToLowerInvariant();
        return await db.Users
            .Where(u => EF.Functions.ILike(u.Username, $"%{lower}%") || u.Email.Contains(lower))
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlySet<string>> GetExistingUsernamesAsync(
        IEnumerable<string> usernames, CancellationToken ct = default)
    {
        var list = usernames.ToList();
        var found = await db.Users
            .Where(u => list.Contains(u.Username))
            .Select(u => u.Username)
            .ToListAsync(ct);
        return new HashSet<string>(found, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyDictionary<Guid, User>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var list = ids.ToList();
        var users = await db.Users.Where(u => list.Contains(u.Id)).ToListAsync(ct);
        return users.ToDictionary(u => u.Id);
    }

    public async Task<IReadOnlyDictionary<string, User>> GetByUsernamesAsync(
        IEnumerable<string> usernames, CancellationToken ct = default)
    {
        var list = usernames.ToList();
        var users = await db.Users.Where(u => list.Contains(u.Username)).ToListAsync(ct);
        return users.ToDictionary(u => u.Username, StringComparer.OrdinalIgnoreCase);
    }
}
