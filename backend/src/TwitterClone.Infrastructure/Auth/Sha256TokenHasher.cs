using System.Security.Cryptography;
using System.Text;
using TwitterClone.Application.Interfaces;

namespace TwitterClone.Infrastructure.Auth;

internal sealed class Sha256TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
