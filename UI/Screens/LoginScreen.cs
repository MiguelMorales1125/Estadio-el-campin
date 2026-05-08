using Spectre.Console;
using StadiumSystem.Controllers;
using StadiumSystem.Domain.Entities;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Screens;

public static class LoginScreen
{
    public static User? Show(AuthController authController)
    {
        AnsiConsole.Clear();
        RenderHeader();

        string username = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Theme.Accent("Usuario")}: ")
                .Validate(input => !string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("El usuario no puede estar vacío.")));

        string password = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Theme.Accent("Contraseña")}: ")
                .PromptStyle(Theme.DangerColor)
                .Secret()
                .Validate(input => !string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("La contraseña no puede estar vacía.")));

        User? user = authController.Login(username.Trim(), password);

        if (user is null)
        {
            AnsiConsole.MarkupLine(Theme.Danger("Credenciales inválidas."));
            Pause();
            return null;
        }

        AnsiConsole.MarkupLine(
            $"[{Theme.SuccessColor}]Bienvenido, [bold]{Markup.Escape(user.Username)}[/]. Rol: [bold]{Markup.Escape(user.Role)}[/].[/]");
        Pause();
        return user;
    }

    private static void RenderHeader()
    {
        AnsiConsole.Write(new FigletText("Login").Centered().Color(Theme.HeaderColor));
        AnsiConsole.MarkupLine(Theme.Muted("Ingresa tus credenciales para continuar"));
        AnsiConsole.MarkupLine(string.Empty);
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine(Theme.Muted("Presiona cualquier tecla para volver al menú principal..."));
        Console.ReadKey(true);
    }
}
