using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using StadiumSystem.Controllers;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Domain.Enums;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Screens;

public static class UserManagementScreen
{
    private enum UserModuleOption
    {
        ViewUsers,
        RegisterUser,
        DeleteUsers,
        ViewCommandHistory,
        Back
    }

    public static void Show(UserController userController, User currentUser)
    {
        bool running = true;

        while (running)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Usuarios").Centered().Color(Theme.HeaderColor));
            AnsiConsole.MarkupLine(Theme.Muted("Módulo de administración de usuarios"));
            AnsiConsole.MarkupLine(string.Empty);

            var option = AnsiConsole.Prompt(
                new SelectionPrompt<UserModuleOption>()
                    .Title(Theme.Accent("Seleccione una acción"))
                    .AddChoices(UserModuleOption.ViewUsers, UserModuleOption.RegisterUser, UserModuleOption.DeleteUsers, UserModuleOption.ViewCommandHistory, UserModuleOption.Back)
                    .UseConverter(opt => opt switch
                    {
                        UserModuleOption.ViewUsers => "Ver usuarios actuales",
                        UserModuleOption.RegisterUser => "Registrar usuario nuevo",
                        UserModuleOption.DeleteUsers => "Borrar usuarios",
                        UserModuleOption.ViewCommandHistory => "Ver historial de comandos",
                        UserModuleOption.Back => "Volver",
                        _ => opt.ToString()
                    }));

            switch (option)
            {
                case UserModuleOption.ViewUsers:
                    ShowUsers(userController, currentUser);
                    break;
                case UserModuleOption.RegisterUser:
                    RegisterUser(userController, currentUser);
                    break;
                case UserModuleOption.DeleteUsers:
                    DeleteUsers(userController, currentUser);
                    break;
                case UserModuleOption.ViewCommandHistory:
                    ViewCommandHistory();
                    break;
                case UserModuleOption.Back:
                    running = false;
                    break;
            }
        }
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine(Theme.Muted("Presiona cualquier tecla para continuar..."));
        Console.ReadKey(true);
    }

    private static void ShowUsers(UserController userController, User currentUser)
    {
        AnsiConsole.Clear();
        var users = userController.GetUsers(currentUser);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("Usuario")
            .AddColumn("Rol");

        foreach (var user in users)
        {
            table.AddRow(user.Id.ToString(), user.Username, user.Role);
        }

        AnsiConsole.Write(table);
        Pause();
    }

    private static void RegisterUser(UserController userController, User currentUser)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine(Theme.Muted("Escribe 'cancel' en cualquier campo para cancelar."));
        AnsiConsole.MarkupLine(string.Empty);

        string username = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Theme.Accent("Usuario")}: ")
                .Validate(v => !string.IsNullOrWhiteSpace(v)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("El usuario no puede estar vacío.")));

        if (string.Equals(username.Trim(), "cancel", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(Theme.Muted("Registro cancelado."));
            Pause();
            return;
        }

        string password = AnsiConsole.Prompt(
            new TextPrompt<string>($"{Theme.Accent("Contraseña")}: ")
                .Secret()
                .Validate(v => !string.IsNullOrWhiteSpace(v)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("La contraseña no puede estar vacía.")));

        if (string.Equals(password.Trim(), "cancel", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(Theme.Muted("Registro cancelado."));
            Pause();
            return;
        }

        List<string> roleOptions = new() { Roles.OPERATOR.ToString() };

        var adminExists = false;
        try
        {
            using var db = new AppDbContext();
            adminExists = db.Users.AsNoTracking().Any(u => u.Role == Roles.ADMIN.ToString());
        }
        catch
        {
        }

        if (!adminExists)
        {
            roleOptions.Insert(0, Roles.ADMIN.ToString());
        }
        roleOptions.Add("Cancelar");

        string roleChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Theme.Accent("Rol"))
                .AddChoices(roleOptions));

        if (string.Equals(roleChoice, "Cancelar", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(Theme.Muted("Registro cancelado."));
            Pause();
            return;
        }

        if (adminExists && string.Equals(roleChoice, Roles.ADMIN.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(Theme.Danger("Error: Ya existe un administrador en el sistema."));
            AnsiConsole.MarkupLine(Theme.Muted("Solo puede haber un administrador."));
            Pause();
            return;
        }

        var result = userController.RegisterUser(username, password, roleChoice, currentUser);
        AnsiConsole.MarkupLine(result.Success ? Theme.Success(result.Message) : Theme.Danger(result.Message));
        Pause();
    }

    private static void DeleteUsers(UserController userController, User currentUser)
    {
        AnsiConsole.Clear();

        var users = userController.GetUsers(currentUser)
            .Where(u => u.Id != currentUser.Id)
            .ToList();

        if (users.Count == 0)
        {
            AnsiConsole.MarkupLine(Theme.Warning("No hay usuarios disponibles para borrar."));
            Pause();
            return;
        }

        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<User>()
                .Title(Theme.Accent("Seleccione uno o más usuarios para borrar"))
                .NotRequired()
                .PageSize(12)
                .InstructionsText("[grey](Espacio para seleccionar, Enter para confirmar)[/]")
                .UseConverter(u =>
                    $"ID {u.Id} - {Markup.Escape(u.Username)} ({Markup.Escape(u.Role)})")
                .AddChoices(users));

        if (selected.Count == 0)
        {
            AnsiConsole.MarkupLine(Theme.Warning("No se seleccionaron usuarios."));
            Pause();
            return;
        }

        string selectedNames = string.Join(", ", selected.Select(u => u.Username));
        bool confirmed = AnsiConsole.Confirm($"¿Confirmas borrar {selected.Count} usuario(s)? {selectedNames}", false);
        if (!confirmed)
        {
            AnsiConsole.MarkupLine(Theme.Muted("Operación cancelada."));
            Pause();
            return;
        }

        var ids = selected.Select(u => u.Id).ToArray();
        var result = userController.DeleteUsers(ids, currentUser);
        AnsiConsole.MarkupLine(result.Success ? Theme.Success(result.Message) : Theme.Danger(result.Message));
        Pause();
    }

    private static void ViewCommandHistory()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Historial de Comandos").Centered().Color(Theme.HeaderColor));
        AnsiConsole.MarkupLine(string.Empty);

        try
        {
            using var db = new AppDbContext();

            var users = db.Users.AsNoTracking().OrderBy(u => u.Username).ToList();

            if (!users.Any())
            {
                AnsiConsole.MarkupLine(Theme.Muted("No hay usuarios en el sistema."));
                Pause();
                return;
            }

            var userOptions = users.Select(u => u.Username).ToList();
            userOptions.Insert(0, "* Todos los usuarios");

            string selectedUser = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(Theme.Accent("Filtrar por usuario"))
                    .AddChoices(userOptions));

            bool useDateFilter = AnsiConsole.Confirm(
                Theme.Accent("¿Deseas filtrar por rango de fechas?"),
                false);

            DateTime? startDate = null;
            DateTime? endDate = null;

            if (useDateFilter)
            {
                string startDateStr = AnsiConsole.Prompt(
                    new TextPrompt<string>($"{Theme.Accent("Fecha inicial (yyyy-MM-dd)")} o presiona Enter: ")
                        .AllowEmpty());

                if (!string.IsNullOrWhiteSpace(startDateStr) && DateTime.TryParse(startDateStr, out var parsedStart))
                {
                    startDate = parsedStart.Date;
                }

                string endDateStr = AnsiConsole.Prompt(
                    new TextPrompt<string>($"{Theme.Accent("Fecha final (yyyy-MM-dd)")} o presiona Enter: ")
                        .AllowEmpty());

                if (!string.IsNullOrWhiteSpace(endDateStr) && DateTime.TryParse(endDateStr, out var parsedEnd))
                {
                    endDate = parsedEnd.Date.AddDays(1).AddTicks(-1);
                }
            }

            var query = db.AuditLogs.AsNoTracking();

            if (selectedUser != "* Todos los usuarios")
            {
                query = query.Where(a => a.User != null && a.User.Username == selectedUser);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= endDate.Value);
            }

            var logs = query.Include(a => a.User).OrderByDescending(a => a.Timestamp).ToList();

            if (!logs.Any())
            {
                AnsiConsole.MarkupLine(Theme.Muted("No hay comandos registrados con estos filtros."));
                Pause();
                return;
            }

            var table = new Table()
                .AddColumn(Theme.Accent("Fecha"))
                .AddColumn(Theme.Accent("Usuario"))
                .AddColumn(Theme.Accent("Categoría"))
                .AddColumn(Theme.Accent("Comando"))
                .AddColumn(Theme.Accent("Detalles"))
                .Border(TableBorder.Rounded)
                .BorderStyle(new Style(Theme.Current.Accent));

            foreach (var log in logs)
            {
                string timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                string username = Markup.Escape(log.User?.Username ?? "Desconocido");
                string category = log.CommandCategory ?? "N/A";
                string commandType = Markup.Escape(log.CommandType ?? "N/A");
                string additionalData = string.IsNullOrWhiteSpace(log.AdditionalData)
                    ? "-"
                    : Markup.Escape(log.AdditionalData.Length > 80
                        ? log.AdditionalData[..80] + "..."
                        : log.AdditionalData);

                string categoryColor = category switch
                {
                    "HARDWARE" => "red",
                    "DATABASE" => "yellow",
                    "ADMIN" => "magenta",
                    "AUTH" => "cyan",
                    _ => "white"
                };

                table.AddRow(
                    Markup.Escape(timestamp),
                    username,
                    $"[{categoryColor}]{category}[/]",
                    commandType,
                    additionalData);
            }

            AnsiConsole.Write(new Panel(table)
                .Border(BoxBorder.Rounded)
                .BorderStyle(new Style(Theme.Current.Accent))
                .Padding(1, 1, 1, 1));

            AnsiConsole.MarkupLine(string.Empty);
            AnsiConsole.MarkupLine(Theme.Muted($"Total de registros: {logs.Count}"));
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine(Theme.Danger($"Error al cargar el historial: {ex.Message}"));
        }

        Pause();
    }
}
