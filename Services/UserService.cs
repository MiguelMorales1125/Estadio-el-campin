using System;
using System.Collections.Generic;
using System.Linq;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Domain.Enums;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Infrastructure.Security;

namespace StadiumSystem.Services;

public sealed class UserService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public List<User> GetUsers(User currentUser)
    {
        if (!IsAdmin(currentUser)) return new List<User>();

        return _dbContext.Users
            .OrderBy(u => u.Role)
            .ThenBy(u => u.Username)
            .ToList();
    }

    public (bool Success, string Message) RegisterUser(string username, string password, string role, User currentUser)
    {
        if (!IsAdmin(currentUser)) return (false, "Solo un ADMIN puede registrar usuarios.");
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return (false, "Usuario y contraseña son obligatorios.");

        string normalizedUsername = username.Trim();
        if (_dbContext.Users.Any(u => u.Username == normalizedUsername))
            return (false, "El nombre de usuario ya existe.");

        if (!Enum.TryParse<Roles>(role, true, out var parsedRole))
            return (false, "Rol inválido.");

        var newUser = new User
        {
            Username = normalizedUsername,
            PasswordHash = _passwordHasher.HashPassword(password),
            Role = parsedRole.ToString()
        };

        _dbContext.Users.Add(newUser);
        _dbContext.SaveChanges();
        return (true, $"Usuario '{newUser.Username}' creado correctamente.");
    }

    public (bool Success, string Message) DeleteUsers(IReadOnlyCollection<int> userIds, User currentUser)
    {
        if (!IsAdmin(currentUser)) return (false, "Solo un ADMIN puede borrar usuarios.");
        if (userIds.Count == 0) return (false, "No seleccionaste usuarios para borrar.");

        if (userIds.Contains(currentUser.Id))
            return (false, "No puedes borrar tu propio usuario desde este módulo.");

        var usersToDelete = _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToList();

        if (usersToDelete.Count == 0)
            return (false, "No se encontraron usuarios para borrar.");

        int totalAdmins = _dbContext.Users.Count(u => u.Role == Roles.ADMIN.ToString());
        int selectedAdmins = usersToDelete.Count(u => u.Role == Roles.ADMIN.ToString());
        if (selectedAdmins > 0 && selectedAdmins >= totalAdmins)
            return (false, "No puedes borrar todos los administradores del sistema.");

        _dbContext.Users.RemoveRange(usersToDelete);
        _dbContext.SaveChanges();
        return (true, $"Se borraron {usersToDelete.Count} usuario(s).");
    }

    private static bool IsAdmin(User user)
    {
        return string.Equals(user.Role, Roles.ADMIN.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.Role, "ADMINISTRADOR", StringComparison.OrdinalIgnoreCase);
    }
}
