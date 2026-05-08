using StadiumSystem.Domain.Entities;
using StadiumSystem.Services;

namespace StadiumSystem.Controllers;

public class AuthController
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    public void SeedAdminIfNotExists() => _authService.SeedAdminIfNotExists();

    public User? Login(string username, string password) => _authService.Login(username, password);

    public bool RegisterUser(string newUsername, string newPassword, string role, User currentUser)
        => _authService.RegisterUser(newUsername, newPassword, role, currentUser);
}
