namespace StadiumSystem.Infrastructure.Security;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string inputPassword, string storedHash);
}
