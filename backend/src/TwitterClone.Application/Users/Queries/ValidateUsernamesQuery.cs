using TwitterClone.Application.Interfaces;

namespace TwitterClone.Application.Users.Queries;

public sealed record ValidateUsernamesQuery(IEnumerable<string> Usernames);

public sealed class ValidateUsernamesHandler(IUserRepository users)
{
    private const int MaxUsernames = 50;

    public async Task<IReadOnlyDictionary<string, bool>> HandleAsync(
        ValidateUsernamesQuery query, CancellationToken ct = default)
    {
        var input = query.Usernames.Take(MaxUsernames).ToList();
        if (input.Count == 0)
        {
            return new Dictionary<string, bool>();
        }

        var existing = await users.GetExistingUsernamesAsync(input, ct);
        return input.ToDictionary(u => u, u => existing.Contains(u));
    }
}
