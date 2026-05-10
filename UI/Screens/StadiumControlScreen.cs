using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using StadiumSystem.Controllers;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Domain.Enums;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Services;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Screens;

public static class StadiumControlScreen
{
    public static void Show(User currentUser, LightController lightController)
    {
        bool running = true;

        while (running)
        {
            AnsiConsole.Clear();
            RenderHeader();

            var (stadiumMode, activeMatch) = GetCurrentState();
            RenderCurrentState(stadiumMode, activeMatch);

            var options = new[] { "Cambiar estado del estadio", "Encender LEDs", "Apagar LEDs", "Información de estado", "Volver" };
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
                    ChangeStadiumState(currentUser);
                    break;
                case 1:
                    TurnLightsOn(lightController);
                    break;
                case 2:
                    TurnLightsOff(lightController);
                    break;
                case 3:
                    ShowStateInformation();
                    break;
                case 4:
                    running = false;
                    break;
            }
        }
    }

    private static void TurnLightsOn(LightController lightController)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("LEDs Encendidos").Centered().Color(Theme.HeaderColor));

        lightController.TurnLightsOn();

        AnsiConsole.MarkupLine(Theme.Success("✓ Se enviaron comandos para encender los LEDs."));
        Pause();
    }

    private static void TurnLightsOff(LightController lightController)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("LEDs Apagados").Centered().Color(Theme.HeaderColor));

        lightController.TurnLightsOff();

        AnsiConsole.MarkupLine(Theme.Success("✓ Se enviaron comandos para apagar los LEDs."));
        Pause();
    }

    private static void ChangeStadiumState(User currentUser)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Cambiar Estado").Centered().Color(Theme.HeaderColor));

        var (currentMode, activeMatch) = GetCurrentState();

        AnsiConsole.MarkupLine($"Estado actual: [bold]{Markup.Escape(currentMode)}[/]");
        AnsiConsole.MarkupLine(string.Empty);

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

        string? validationError = ValidateStateChange(currentMode, selectedState, activeMatch);
        if (validationError is not null)
        {
            AnsiConsole.MarkupLine(Theme.Danger($"Error: {validationError}"));
            Pause();
            return;
        }

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
                var auditLogService = new AuditLogService(db);
                var match = db.Matches.FirstOrDefault(m => m.Id == activeMatch.Id);
                if (match is not null)
                {
                    match.IsActive = false;
                    match.FinishedAt = DateTime.UtcNow;
                }

                var stadiumState = db.StadiumStates.FirstOrDefault(s => s.Id == 1);
                if (stadiumState is not null)
                {
                    stadiumState.Mode = Enum.Parse<StadiumStates>(selectedState);
                }

                db.SaveChanges();
                auditLogService.Log(currentUser, "CHANGE_STATE", CommandCategory.DATABASE, $"Estado del estadio: {currentMode} -> {selectedState}. Partido finalizado: {(activeMatch is not null ? activeMatch.TeamLocal + " vs " + activeMatch.TeamAway : "N/A")}");

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
                var auditLogService = new AuditLogService(db);
                var stadiumState = db.StadiumStates.FirstOrDefault(s => s.Id == 1);
                string previousState = stadiumState?.Mode.ToString() ?? currentMode;

                if (stadiumState is not null)
                {
                    stadiumState.Mode = Enum.Parse<StadiumStates>(selectedState);
                }
                else
                {
                    db.StadiumStates.Add(new StadiumState { Id = 1, Mode = Enum.Parse<StadiumStates>(selectedState) });
                }

                db.SaveChanges();
                auditLogService.Log(currentUser, "CHANGE_STATE", CommandCategory.DATABASE, $"Estado del estadio: {previousState} -> {selectedState}");

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
                available.Add("ACTIVO");
                available.Add("MANTENIMIENTO");
                break;

            case "ACTIVO":
                available.Add("DISPONIBLE");
                available.Add("MANTENIMIENTO");
                break;

            case "MANTENIMIENTO":
                available.Add("DISPONIBLE");
                break;

            default:
                available.Add("DISPONIBLE");
                break;
        }

        return available;
    }

    private static string? ValidateStateChange(string currentMode, string newState, MatchSession? activeMatch)
    {
        if (newState == "ACTIVO" && activeMatch is null)
        {
            return "No hay un partido activo para activar el estadio.";
        }

        if (currentMode == newState)
        {
            return "El estado ya es ese.";
        }

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

            string mode = stadiumState?.Mode.ToString() ?? "DISPONIBLE";

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
