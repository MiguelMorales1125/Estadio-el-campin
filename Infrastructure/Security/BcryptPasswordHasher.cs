namespace StadiumSystem.Infrastructure.Security;

/// <summary>
/// GRASP: Pure Fabrication ultra simplificado usando BCrypt.
/// BCrypt maneja internamente la encriptación y el "salt" en un solo string,
/// ahorrándonos 40 líneas de código y reduciendo la complejidad drásticamente.
/// </summary>
public class BcryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // Genera la "sal" y el hash automáticamente y los concatena por ti.
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string inputPassword, string storedHash)
    {
        // Lee el string de tu BD, saca el salt oculto, lo prueba y retorna true o false.
        return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
    }
}
