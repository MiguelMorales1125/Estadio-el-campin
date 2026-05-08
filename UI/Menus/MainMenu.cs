using Spectre.Console;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Menus;

public enum MainMenuOption
{
    Login,
    Exit
}

public static class MainMenu
{
    public static MainMenuOption Show()
    {
        AnsiConsole.Clear();
        DrawHeader();

        var option = AnsiConsole.Prompt(
            new SelectionPrompt<MainMenuOption>()
                .Title(Theme.Accent("Seleccione una opción"))
                .PageSize(10)
                .AddChoices(
                    MainMenuOption.Login,
                    MainMenuOption.Exit));

        return option;
    }

    private static void DrawHeader()
    {
        AnsiConsole.Write(new FigletText("Estadio el Campin").Centered().Color(Theme.HeaderColor));
        AnsiConsole.MarkupLine(Theme.Muted("Sistema de control del estadio"));
        AnsiConsole.MarkupLine(string.Empty);
    }
}
