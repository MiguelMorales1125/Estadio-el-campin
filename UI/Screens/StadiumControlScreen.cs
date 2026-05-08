using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Screens;

public static class StadiumControlScreen
{
    public static void Show(User currentUser)
    {
        bool running = true;

        while (running)
        {
            AnsiConsole.Clear();
            RenderHeader();

            var (stadiumMode, activeMatch) = GetCurrentState();
            RenderCurrentState(stadiumMode, activeMatch);

            var options = new[] { "Cambiar estado del estadio", "Información de estado", "Volver" };
            int selected = 0;

            bool choosingOption = true;
            while (choosingOption)
            {
                RenderOptions(options, selected);
                RenderHelp();

                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        selected = (selected - 1 + options.Length) % options.Length;
                        AnsiConsole.Clear();
                        RenderHeader();
                        var state = GetCurrentState();
                        RenderCurrentState(state.StadiumMode, state.ActiveMatch);
                        break;
                    case ConsoleKey.DownArrow:
                        selected = (selected + 1) % options.Length;
                        AnsiConsole.Clear();
                        RenderHeader();
                        state = GetCurrentState();
                        RenderCurrentState(state.StadiumMode, state.ActiveMatch);
                        break;
                    case ConsoleKey.Enter:
                        choosingOption = false;
                        break;
                    case ConsoleKey.Escape:
                        running = false;
                        choosingOption = false;
                        break;
                }
            }

            if (!running) break;

            switch (selected)
            {
                case 0:
                    ChangeStadiumState();
                    break;
                case 1:
                    ShowStateInformation();
                    break;
                case 2:
                    running = false;
                    break;
            }
        }
    }

    private static void ChangeStadiumState()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Cambiar Estado").Centered().Color(Theme.HeaderColor));

        var (currentMode, activeMatch) = GetCurrentState();

        AnsiConsole.MarkupLine($"Estado actual: [bold]{Markup.Escape(currentMode)}[/]");
        AnsiConsole.MarkupLine(string.Empty);

        // Determine available options based on current state and match
        var availableStates = DetermineAvailableStates(currentMode, activeMatch);

        if (availableStates.Count == 0)
        {
            AnsiConsole.MarkupLine(Theme.Danger("No hay cambios de estado disponibles en este momento."));
            if (currentMode == "MANTENIMIENTO")
            {
                AnsiConsole.MarkupLine(Theme.Muted("El estadio está en mantenimiento. No se pueden realizar cambios."));
            }
            Pause();
            return;
        }

        string selectedState = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"{Theme.Accent("Nuevo estado")}")
                .AddChoices(availableStates)
                .UseConverter(s => FormatStateLabel(s, currentMode, availableStates.IndexOf(s))));

        if (string.IsNullOrEmpty(selectedState))
        {
            AnsiConsole.MarkupLine(Theme.Muted("Operación cancelada."));
            Pause();
            return;
        }

        // Validate state change
        string? validationError = ValidateStateChange(currentMode, selectedState, activeMatch);
        if (validationError is not null)
        {
            AnsiConsole.MarkupLine(Theme.Danger($"Error: {validationError}"));
            Pause();
            return;
        }

        // Handle state change for ACTIVO -> DISPONIBLE (finish match)
        if (currentMode == "ACTIVO" && selectedState == "DISPONIBLE" && activeMatch is not null)
        {
            bool confirm = AnsiConsole.Confirm(
                Theme.Accent("¿Deseas finalizar el partido actual?"),
                false);

            if (!confirm)
            {
                AnsiConsole.MarkupLine(Theme.Muted("Operación cancelada."));
                Pause();
                return;
            }

            try
            {
                using var db = new AppDbContext();
                var match = db.Matches.FirstOrDefault(m => m.Id == activeMatch.Id);
                if (match is not null)
                {
                    match.IsActive = false;
                    match.FinishedAt = DateTime.UtcNow;
                }

                var stadiumState = db.StadiumStates.FirstOrDefault(s => s.Id == 1);
                if (stadiumState is not null)
                {
                    stadiumState.Mode = selectedState;
                }

                db.SaveChanges();

                AnsiConsole.MarkupLine(Theme.Success("✓ Partido finalizado."));
                AnsiConsole.MarkupLine(Theme.Success($"✓ Estado del estadio cambiado a: {Markup.Escape(selectedState)}"));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Theme.Danger($"Error: {Markup.Escape(ex.Message)}"));
            }
        }
        else
        {
            try
            {
                using var db = new AppDbContext();
                var stadiumState = db.StadiumStates.FirstOrDefault(s => s.Id == 1);

                if (stadiumState is not null)
                {
                    stadiumState.Mode = selectedState;
                }
                else
                {
                    db.StadiumStates.Add(new StadiumState { Id = 1, Mode = selectedState });
                }

                db.SaveChanges();

                AnsiConsole.MarkupLine(Theme.Success($"✓ Estado del estadio cambiado a: {Markup.Escape(selectedState)}"));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine(Theme.Danger($"Error: {Markup.Escape(ex.Message)}"));
            }
        }

        Pause();
    }

    private static void ShowStateInformation()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Información de Estados").Centered().Color(Theme.HeaderColor));

        var infoTable = new Table()
            .AddColumn(Theme.Accent("Estado"))
            .AddColumn(Theme.Accent("Descripción"))
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Theme.Current.Accent));

        infoTable.AddRow(
            "[bold]DISPONIBLE[/]",
            "El estadio está disponible y sin partidos activos. Se pueden crear nuevos partidos.");

        infoTable.AddRow(
            "[bold]ACTIVO[/]",
            "Hay un partido en vivo. El estadio está operativo y no se pueden crear nuevos partidos\nhasta que el actual termine.");

        infoTable.AddRow(
            "[bold]MANTENIMIENTO[/]",
            "El estadio está en mantenimiento. No hay partidos activos y no se pueden crear nuevos\nhasta que el mantenimiento termine.");

        AnsiConsole.Write(new Panel(infoTable)
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Theme.Current.Accent))
            .Padding(1, 1, 1, 1));

        Pause();
    }

    private static List<string> DetermineAvailableStates(string currentMode, MatchSession? activeMatch)
    {
        var available = new List<string>();

        switch (currentMode)
        {
            case "DISPONIBLE":
                available.Add("ACTIVO");    // Can start a match
                available.Add("MANTENIMIENTO"); // Can go to maintenance
                break;

            case "ACTIVO":
                available.Add("DISPONIBLE"); // Can finish the match
                available.Add("MANTENIMIENTO"); // Can go to maintenance (will finish match)
                break;

            case "MANTENIMIENTO":
                available.Add("DISPONIBLE"); // Can exit maintenance
                break;

            default:
                available.Add("DISPONIBLE"); // Default: reset to available
                break;
        }

        return available;
    }

    private static string? ValidateStateChange(string currentMode, string newState, MatchSession? activeMatch)
    {
        // Can't go to ACTIVO without an active match
        if (newState == "ACTIVO" && activeMatch is null)
        {
            return "No hay un partido activo para activar el estadio.";
        }

        // Can't stay in same state
        if (currentMode == newState)
        {
            return "El estado ya es ese.";
        }

        // Can't go to MANTENIMIENTO with an active match without confirmation
        // (confirmation is handled in ChangeStadiumState)

        return null;
    }

    private static string FormatStateLabel(string state, string currentMode, int index)
    {
        string baseLabel = state switch
        {
            "DISPONIBLE" => "• Disponible (sin partidos activos)",
            "ACTIVO" => "• Activo (partido en vivo)",
            "MANTENIMIENTO" => "• Mantenimiento",
            _ => state
        };

        return baseLabel;
    }

    private static (string StadiumMode, MatchSession? ActiveMatch) GetCurrentState()
    {
        try
        {
            using var db = new AppDbContext();

            var stadiumState = db.StadiumStates
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == 1);

            string mode = stadiumState?.Mode ?? "DISPONIBLE";

            var activeMatch = db.Matches
                .AsNoTracking()
                .FirstOrDefault(m => m.IsActive);

            return (mode, activeMatch);
        }
        catch
        {
            return ("?", null);
        }
    }

    private static void RenderHeader()
    {
        AnsiConsole.Write(new FigletText("Control del Estadio").Centered().Color(Theme.HeaderColor));
        AnsiConsole.MarkupLine(string.Empty);
    }

    private static void RenderCurrentState(string mode, MatchSession? activeMatch)
    {
        string stateColor = mode switch
        {
            "ACTIVO" => "bold green",
            "DISPONIBLE" => "bold cyan",
            "MANTENIMIENTO" => "bold yellow",
            _ => "grey"
        };

        string matchInfo = activeMatch is null
            ? "[yellow]SIN PARTIDO ACTIVO[/]"
            : $"[green]{Markup.Escape(activeMatch.TeamLocal)}[/] vs [green]{Markup.Escape(activeMatch.TeamAway)}[/]";

        var status = new Markup(
            $"[{Theme.Current.Muted}]Modo:[/] [{stateColor}]{Markup.Escape(mode)}[/]   " +
            $"[{Theme.Current.Muted}]Partido:[/] {matchInfo}");

        AnsiConsole.Write(new Panel(status)
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Theme.Current.Muted))
            .Padding(1, 0, 1, 0));

        AnsiConsole.MarkupLine(string.Empty);
    }

    private static void RenderOptions(string[] options, int selected)
    {
        AnsiConsole.MarkupLine(Theme.Accent("Opciones"));

        for (int i = 0; i < options.Length; i++)
        {
            bool isSelected = i == selected;
            string prefix = isSelected ? ">" : " ";
            string color = isSelected ? Theme.Current.Accent.ToString() : Theme.Current.Muted.ToString();
            AnsiConsole.MarkupLine($"[{color}]{prefix} {options[i]}[/]");
        }

        AnsiConsole.MarkupLine(string.Empty);
    }

    private static void RenderHelp()
    {
        AnsiConsole.MarkupLine(Theme.Muted("Usa ↑/↓ para navegar, Enter para seleccionar, Esc para volver."));
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine(Theme.Muted("Presiona cualquier tecla para volver..."));
        Console.ReadKey(true);
    }
}
