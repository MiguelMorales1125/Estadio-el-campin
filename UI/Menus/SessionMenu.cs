using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Domain.Enums;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Menus;

public enum SessionMenuOption
{
    MatchManagement,
    StadiumControl,
    TerminalLogs,
    Users,
    Logout,
    Exit
}

public static class SessionMenu
{
    public static SessionMenuOption Show(User user)
    {
        var options = BuildMenuOptions(user);
        int selected = 0;

        while (true)
        {
            LiveMenuSnapshot snapshot = GetLiveSnapshot();

            AnsiConsole.Clear();
            RenderHeader(user);
            RenderLiveStatus(snapshot);
            RenderOptions(options, selected);
            RenderHelp();

            var handledKey = WaitForKeyPressWithRefresh(timeoutMs: 500);
            if (handledKey is null)
            {
                continue;
            }

            switch (handledKey.Value.Key)
            {
                case ConsoleKey.UpArrow:
                    selected = (selected - 1 + options.Count) % options.Count;
                    break;
                case ConsoleKey.DownArrow:
                    selected = (selected + 1) % options.Count;
                    break;
                case ConsoleKey.Enter:
                    return options[selected].Option;
                case ConsoleKey.Escape:
                    return SessionMenuOption.Logout;
            }
        }
    }

    public static void ShowComingSoon(string moduleName)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Próximamente").Centered().Color(Theme.HeaderColor));
        AnsiConsole.Write(
            new Panel(new Markup($"[bold]{Markup.Escape(moduleName)}[/]\n\n[grey]Este módulo aún no tiene funcionalidad implementada.[/]"))
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(Theme.Current.Accent))
                .Header(Theme.Accent("En construcción"), Justify.Center)
                .Padding(1, 1, 1, 1));
        Pause();
    }

    private static List<MenuItem> BuildMenuOptions(User user)
    {
        bool isAdmin = string.Equals(user.Role, "ADMIN", StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.Role, "ADMINISTRADOR", StringComparison.OrdinalIgnoreCase);

        var items = new List<MenuItem>
        {
            new(SessionMenuOption.MatchManagement, "Gestión de Partidos"),
            new(SessionMenuOption.StadiumControl, "Control del Estadio"),
            new(SessionMenuOption.TerminalLogs, "Ver logs de terminal")
        };

        if (isAdmin)
        {
            items.Add(new MenuItem(SessionMenuOption.Users, "Administración"));
        }

        items.Add(new MenuItem(SessionMenuOption.Logout, "Cerrar sesión"));
        items.Add(new MenuItem(SessionMenuOption.Exit, "Salir"));

        return items;
    }

    private static LiveMenuSnapshot GetLiveSnapshot()
    {
        try
        {
            using var db = new AppDbContext();

            string mode = db.StadiumStates
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == 1)?.Mode.ToString() ?? "APAGADO";

            var activeMatch = db.Matches
                .AsNoTracking()
                .FirstOrDefault(m => m.IsActive);

            if (activeMatch is null)
            {
                return new LiveMenuSnapshot(
                    StadiumMode: mode,
                    MatchActive: false,
                    ScoreText: "NINGUNO 0 - 0 NINGUNO");
            }

            string score = $"{activeMatch.TeamLocal} {activeMatch.ScoreLocal} - {activeMatch.ScoreAway} {activeMatch.TeamAway}";
            return new LiveMenuSnapshot(
                StadiumMode: mode,
                MatchActive: true,
                ScoreText: score);
        }
        catch
        {
            return new LiveMenuSnapshot(
                StadiumMode: "?",
                MatchActive: false,
                ScoreText: "Sin datos (error de conexión)");
        }
    }

    private static void RenderHeader(User user)
    {
        AnsiConsole.Write(new FigletText("Panel de Control").Centered().Color(Theme.HeaderColor));
        AnsiConsole.MarkupLine(
            $"[{Theme.Current.Muted}]Bienvenido, [bold]{Markup.Escape(user.Username)}[/].[/]");
        AnsiConsole.MarkupLine(string.Empty);
    }

    private static void RenderLiveStatus(LiveMenuSnapshot snapshot)
    {
        string matchState = snapshot.MatchActive ? "SI" : "NO";

        var status = new Markup(
            $"[{Theme.Current.Muted}]ESTADO:[/] [bold]{Markup.Escape(snapshot.StadiumMode)}[/]   " +
            $"[{Theme.Current.Muted}]PARTIDO EN VIVO:[/] [bold]{matchState}[/]   " +
            $"[{Theme.Current.Muted}]MARCADOR:[/] [bold]{Markup.Escape(snapshot.ScoreText)}[/]");

        AnsiConsole.Write(new Panel(status)
            .Border(BoxBorder.Rounded)
            .BorderStyle(new Style(Theme.Current.Muted))
            .Padding(1, 0, 1, 0));

        AnsiConsole.MarkupLine(string.Empty);
    }

    private static void RenderOptions(List<MenuItem> options, int selected)
    {
        AnsiConsole.MarkupLine(Theme.Accent("Opciones"));

        for (int i = 0; i < options.Count; i++)
        {
            bool isSelected = i == selected;
            string prefix = isSelected ? ">" : " ";
            string color = isSelected ? Theme.Current.Accent.ToString() : Theme.Current.Muted.ToString();
            string label = Markup.Escape(options[i].Label);
            AnsiConsole.MarkupLine($"[{color}]{prefix} {label}[/]");
        }

        AnsiConsole.MarkupLine(string.Empty);
    }

    private static void RenderHelp()
    {
        AnsiConsole.MarkupLine(Theme.Muted("Usa ↑/↓ para navegar, Enter para seleccionar, Esc para cerrar sesión."));
        AnsiConsole.MarkupLine(Theme.Muted("El estado se actualiza automáticamente cada 0.5s."));
    }

    private static ConsoleKeyInfo? WaitForKeyPressWithRefresh(int timeoutMs)
    {
        int elapsed = 0;
        const int stepMs = 50;

        while (elapsed < timeoutMs)
        {
            if (Console.KeyAvailable)
            {
                return Console.ReadKey(true);
            }

            Thread.Sleep(stepMs);
            elapsed += stepMs;
        }

        return null;
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine(Theme.Muted("Presiona cualquier tecla para volver..."));
        Console.ReadKey(true);
    }

    private sealed record MenuItem(SessionMenuOption Option, string Label);

    private sealed record LiveMenuSnapshot(
        string StadiumMode,
        bool MatchActive,
        string ScoreText);
}
