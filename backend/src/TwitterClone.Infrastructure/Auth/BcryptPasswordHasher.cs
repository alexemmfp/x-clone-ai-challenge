using TwitterClone.Application.Interfaces;

namespace TwitterClone.Infrastructure.Auth;

internal sealed class BcryptPasswordHasher(int workFactor) : IPasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
