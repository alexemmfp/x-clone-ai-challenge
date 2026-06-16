namespace TwitterClone.Application.Auth.Dtos;

public sealed record AuthResult(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Username);
