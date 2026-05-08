using System.Collections.Generic;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Services;

namespace StadiumSystem.Controllers;

public sealed class UserController
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    public List<User> GetUsers(User currentUser) => _userService.GetUsers(currentUser);

    public (bool Success, string Message) RegisterUser(string username, string password, string role, User currentUser)
        => _userService.RegisterUser(username, password, role, currentUser);

    public (bool Success, string Message) DeleteUsers(IReadOnlyCollection<int> userIds, User currentUser)
        => _userService.DeleteUsers(userIds, currentUser);
}
