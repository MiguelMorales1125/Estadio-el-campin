using System.Linq;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Infrastructure.Security;

namespace StadiumSystem.Controllers;

/// <summary>
/// GRASP: Controlador de Alta Cohesión. Solo maneja lógica de usuarios (Registro/Login).
/// </summary>
public class AuthController
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    // SOLID: Dependency Inversion
    public AuthController(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public void SeedAdminIfNotExists()
    {
        if (!_dbContext.Users.Any(u => u.Username == "admin"))
        {
            _dbContext.Users.Add(new User 
            { 
                Username = "admin", 
                PasswordHash = _passwordHasher.HashPassword("admin123"), 
                Role = "ADMIN" 
            });
            _dbContext.SaveChanges();
        }
    }

    public User? Login(string username, string password)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);
        if (user == null) return null;

        bool isValid = _passwordHasher.VerifyPassword(password, user.PasswordHash);
        return isValid ? user : null;
    }

    public bool RegisterUser(string newUsername, string newPassword, string role, User currentUser)
    {
        if (currentUser.Role != "ADMIN") return false;
        if (_dbContext.Users.Any(u => u.Username == newUsername)) return false;

        var newUser = new User
        {
            Username = newUsername,
            PasswordHash = _passwordHasher.HashPassword(newPassword),
            Role = role
        };

        _dbContext.Users.Add(newUser);
        _dbContext.SaveChanges();
        return true;
    }
}
