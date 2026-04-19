namespace StadiumSystem.Infrastructure.Security;

/// <summary>
/// SOLID: Interface Segregation & Dependency Inversion.
/// Permite intercambiar el algoritmo de seguridad sin cambiar a los controladores.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string inputPassword, string storedHash);
}
