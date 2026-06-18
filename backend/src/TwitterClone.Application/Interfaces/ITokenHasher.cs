namespace TwitterClone.Application.Interfaces;

public interface ITokenHasher
{
    string Hash(string token);
}
