using System;
using System.Threading;
using Spectre.Console;
using StadiumSystem.Services;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Screens;

public static class TerminalLogsScreen
{
    public static void Show(ITerminalLogService logService)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Terminal Logs").Centered().Color(Theme.HeaderColor));

        var table = new Table()
            .AddColumn("Timestamp (UTC)")
            .AddColumn("Level")
            .AddColumn("Message")
            .Border(TableBorder.Rounded)
            .BorderStyle(new Style(Theme.Current.Muted));

        AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx =>
            {
                while (!Console.KeyAvailable)
                {
                    table.Rows.Clear();
                    var entries = logService.GetAll();
                    foreach (var e in entries)
                    {
                        table.AddRow(e.Timestamp.ToString("s"), e.Level.ToString(), e.Message);
                    }
                    ctx.UpdateTarget(table);
                    ctx.Refresh();
                    Thread.Sleep(500);
                }
            });

        if (Console.KeyAvailable) Console.ReadKey(true);
    }
}
