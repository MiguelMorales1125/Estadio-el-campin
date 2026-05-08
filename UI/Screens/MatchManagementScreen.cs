using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Screens;

public static class MatchManagementScreen
{
    public static void Show(User currentUser)
    {
        bool running = true;

        while (running)
        {
            AnsiConsole.Clear();
            RenderHeader();

            var activeMatch = GetActiveMatch();
            RenderMatchStatus(activeMatch);

            var options = new[] { "Ver partido actual", "Crear nuevo partido", "Actualizar marcador", "Volver" };
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
                        RenderMatchStatus(GetActiveMatch());
                        break;
                    case ConsoleKey.DownArrow:
                        selected = (selected + 1) % options.Length;
                        AnsiConsole.Clear();
                        RenderHeader();
                        RenderMatchStatus(GetActiveMatch());
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
                    ViewCurrentMatch();
                    break;
                case 1:
                    CreateNewMatch();
                    break;
                case 2:
                    UpdateScore();
                    break;
                case 3:
                    running = false;
                    break;
            }
        }
    }

    private static void ViewCurrentMatch()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Partido Actual").Centered().Color(Theme.HeaderColor));

        var match = GetActiveMatch();

        if (match is null)
        {
            AnsiConsole.MarkupLine(Theme.Muted("No hay un partido activo en este momento."));
            Pause();
            return;
        }

        var table = new Table()
            .AddColumn(Theme.Accent("Campo"))
            .AddColumn(Theme.Accent("Valor"))
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Theme.Current.Accent));

        table.AddRow(Theme.Muted("Equipo Local"), $"[bold]{Markup.Escape(match.TeamLocal)}[/]");
        table.AddRow(Theme.Muted("Equipo Visitante"), $"[bold]{Markup.Escape(match.TeamAway)}[/]");
        table.AddRow(Theme.Muted("Marcador Local"), $"[bold]{match.ScoreLocal}[/]");
        table.AddRow(Theme.Muted("Marcador Visitante"), $"[bold]{match.ScoreAway}[/]");
        table.AddRow(Theme.Muted("Iniciado"), $"[bold]{match.CreatedAt:yyyy-MM-dd HH:mm:ss}[/]");
        table.AddRow(Theme.Muted("Estado"), "[bold green]EN VIVO[/]");

        AnsiConsole.Write(new Panel(table)
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Theme.Current.Accent))
            .Padding(1, 1, 1, 1));

        Pause();
    }

    private static void CreateNewMatch()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Nuevo Partido").Centered().Color(Theme.HeaderColor));

        // Check if match already exists
        var existingMatch = GetActiveMatch();
        if (existingMatch is not null)
        {
            AnsiConsole.MarkupLine(Theme.Danger("Error: Ya existe un partido activo."));
            AnsiConsole.MarkupLine(Theme.Muted("Debe terminar el partido actual antes de crear uno nuevo."));
            Pause();
            return;
        }

        // Get team names
        string teamLocal = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Theme.Accent("Equipo Local")}: ")
                .Validate(v => !string.IsNullOrWhiteSpace(v)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("El equipo no puede estar vacío.")));

        if (string.Equals(teamLocal.Trim(), "cancel", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(Theme.Muted("Operación cancelada."));
            Pause();
            return;
        }

        string teamAway = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Theme.Accent("Equipo Visitante")}: ")
                .Validate(v => !string.IsNullOrWhiteSpace(v)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("El equipo no puede estar vacío.")));

        if (string.Equals(teamAway.Trim(), "cancel", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(Theme.Muted("Operación cancelada."));
            Pause();
            return;
        }

        try
        {
            using var db = new AppDbContext();

            var newMatch = new MatchSession
            {
                TeamLocal = teamLocal.Trim(),
                TeamAway = teamAway.Trim(),
                ScoreLocal = 0,
                ScoreAway = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Matches.Add(newMatch);

            // Update stadium state to ACTIVO
            var stadiumState = db.StadiumStates.FirstOrDefault(s => s.Id == 1);
            if (stadiumState is not null)
            {
                stadiumState.Mode = "ACTIVO";
            }
            else
            {
                db.StadiumStates.Add(new StadiumState { Id = 1, Mode = "ACTIVO" });
            }

            db.SaveChanges();

            AnsiConsole.MarkupLine(Theme.Success("✓ Partido creado exitosamente."));
            AnsiConsole.MarkupLine(Theme.Muted($"  {Markup.Escape(teamLocal)} vs {Markup.Escape(teamAway)}"));
            AnsiConsole.MarkupLine(Theme.Muted("  El estado del estadio fue cambiado a: ACTIVO"));
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(Theme.Danger($"Error al crear el partido: {Markup.Escape(ex.Message)}"));
        }

        Pause();
    }

    private static void UpdateScore()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Actualizar Marcador").Centered().Color(Theme.HeaderColor));

        var match = GetActiveMatch();

        if (match is null)
        {
            AnsiConsole.MarkupLine(Theme.Danger("Error: No hay un partido activo."));
            Pause();
            return;
        }

        AnsiConsole.MarkupLine($"Marcador actual: {Markup.Escape(match.TeamLocal)} {match.ScoreLocal} - {match.ScoreAway} {Markup.Escape(match.TeamAway)}");
        AnsiConsole.MarkupLine(string.Empty);

        var teamOptions = new[] { match.TeamLocal, match.TeamAway };
        string selectedTeam = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"{Theme.Accent("¿Cuál equipo anotó?")}")
                .AddChoices(teamOptions)
                .UseConverter(t => Markup.Escape(t)));

        if (string.IsNullOrEmpty(selectedTeam))
        {
            AnsiConsole.MarkupLine(Theme.Muted("Operación cancelada."));
            Pause();
            return;
        }

        int goalsToAdd = AnsiConsole.Prompt(
            new TextPrompt<int>($"{Theme.Accent("¿Cuántos goles?")}: ")
                .Validate(v => v > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("Debe ser un número mayor a 0.")));

        try
        {
            using var db = new AppDbContext();
            var matchToUpdate = db.Matches.FirstOrDefault(m => m.Id == match.Id && m.IsActive);

            if (matchToUpdate is null)
            {
                AnsiConsole.MarkupLine(Theme.Danger("Error: No se encontró el partido."));
                Pause();
                return;
            }

            if (selectedTeam == match.TeamLocal)
            {
                matchToUpdate.ScoreLocal += goalsToAdd;
            }
            else
            {
                matchToUpdate.ScoreAway += goalsToAdd;
            }

            db.SaveChanges();

            AnsiConsole.MarkupLine(Theme.Success("✓ Marcador actualizado."));
            AnsiConsole.MarkupLine(Theme.Muted($"  Nuevo marcador: {Markup.Escape(matchToUpdate.TeamLocal)} {matchToUpdate.ScoreLocal} - {matchToUpdate.ScoreAway} {Markup.Escape(matchToUpdate.TeamAway)}"));
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(Theme.Danger($"Error al actualizar el marcador: {Markup.Escape(ex.Message)}"));
        }

        Pause();
    }

    private static MatchSession? GetActiveMatch()
    {
        try
        {
            using var db = new AppDbContext();
            return db.Matches
                .AsNoTracking()
                .FirstOrDefault(m => m.IsActive);
        }
        catch
        {
            return null;
        }
    }

    private static void RenderHeader()
    {
        AnsiConsole.Write(new FigletText("Gestión de Partidos").Centered().Color(Theme.HeaderColor));
        AnsiConsole.MarkupLine(string.Empty);
    }

    private static void RenderMatchStatus(MatchSession? match)
    {
        if (match is null)
        {
            var noMatch = new Markup($"[{Theme.Current.Muted}]Estado:[/] [yellow]SIN PARTIDO ACTIVO[/]");
            AnsiConsole.Write(new Panel(noMatch)
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(Theme.Current.Muted))
                .Padding(1, 0, 1, 0));
        }
        else
        {
            var matchInfo = new Markup(
                $"[{Theme.Current.Muted}]{Markup.Escape(match.TeamLocal)}[/] [bold green]{match.ScoreLocal}[/] " +
                $"[{Theme.Current.Muted}]-[/] " +
                $"[bold green]{match.ScoreAway}[/] [{Theme.Current.Muted}]{Markup.Escape(match.TeamAway)}[/]");
            AnsiConsole.Write(new Panel(matchInfo)
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(Theme.Current.Accent))
                .Padding(1, 0, 1, 0));
        }

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
