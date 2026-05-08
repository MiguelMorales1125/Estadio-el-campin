using System;
using System.Linq;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Domain.Enums;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Infrastructure.Security;

namespace StadiumSystem.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public void SeedAdminIfNotExists()
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Production";

        string? adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (env != "Development" && string.IsNullOrEmpty(adminPassword))
        {
            Console.WriteLine("Skipping admin seed: not in Development and ADMIN_PASSWORD not set.");
            return;
        }

        if (!_dbContext.Users.Any(u => u.Role == Roles.ADMIN.ToString()))
        {
            string passwordToUse = adminPassword ?? string.Empty;
            if (string.IsNullOrEmpty(passwordToUse))
            {
                passwordToUse = "admin123";
                Console.WriteLine("Warning: DEVELOPMENT ONLY - using default admin password 'admin123' because ADMIN_PASSWORD is not set.");
            }

            _dbContext.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = _passwordHasher.HashPassword(passwordToUse),
                Role = Roles.ADMIN.ToString()
            });
            _dbContext.SaveChanges();
            Console.WriteLine("Admin user created.");
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
        if (currentUser.Role != Roles.ADMIN.ToString()) return false;
        if (_dbContext.Users.Any(u => u.Username == newUsername)) return false;

        // Validate role against enum
        bool validRole = System.Enum.TryParse<Roles>(role, true, out var parsedRole);
        if (!validRole) return false;

        var newUser = new User
        {
            Username = newUsername,
            PasswordHash = _passwordHasher.HashPassword(newPassword),
            Role = parsedRole.ToString()
        };

        _dbContext.Users.Add(newUser);
        _dbContext.SaveChanges();
        return true;
    }
}
