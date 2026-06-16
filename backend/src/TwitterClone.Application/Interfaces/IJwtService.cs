using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
