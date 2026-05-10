using Spectre.Console;
using StadiumSystem.Controllers;
using StadiumSystem.Domain.Entities;
using StadiumSystem.UI.Menus;
using StadiumSystem.UI.Screens;

namespace StadiumSystem.UI;

public sealed class ConsoleApp
{
    private readonly AuthController _authController;
    private readonly UserController _userController;
    private readonly LightController _lightController;
    private readonly StadiumSystem.Services.ITerminalLogService _logService;

    public ConsoleApp(AuthController authController, UserController userController, LightController lightController, StadiumSystem.Services.ITerminalLogService logService)
    {
        _authController = authController;
        _userController = userController;
        _lightController = lightController;
        _logService = logService;
    }

    public void Run()
    {
        _authController.SeedAdminIfNotExists();

        var running = true;
        User? currentUser = null;

        while (running)
        {
            if (currentUser is null)
            {
                var loginResult = MainMenu.Show();

                switch (loginResult)
                {
                    case MainMenuOption.Login:
                        currentUser = LoginScreen.Show(_authController);
                        break;
                    case MainMenuOption.Exit:
                        running = false;
                        break;
                }

                continue;
            }

            var authenticatedUser = currentUser;
            if (authenticatedUser is null)
            {
                continue;
            }

            var sessionOption = SessionMenu.Show(authenticatedUser);

            switch (sessionOption)
            {
                case SessionMenuOption.MatchManagement:
                    MatchManagementScreen.Show(authenticatedUser);
                    break;
                case SessionMenuOption.StadiumControl:
                    StadiumControlScreen.Show(authenticatedUser, _lightController);
                    break;
                case SessionMenuOption.TerminalLogs:
                    StadiumSystem.UI.Screens.TerminalLogsScreen.Show(_logService);
                    break;
                case SessionMenuOption.Users:
                    UserManagementScreen.Show(_userController, authenticatedUser);
                    break;
                case SessionMenuOption.Logout:
                    currentUser = null;
                    break;
                case SessionMenuOption.Exit:
                    running = false;
                    break;
            }
        }
    }
}
