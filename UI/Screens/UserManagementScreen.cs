using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using StadiumSystem.Controllers;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Domain.Enums;
using StadiumSystem.UI.Theming;

namespace StadiumSystem.UI.Screens;

public static class UserManagementScreen
{
    private enum UserModuleOption
    {
        ViewUsers,
        RegisterUser,
        DeleteUsers,
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
                    .AddChoices(UserModuleOption.ViewUsers, UserModuleOption.RegisterUser, UserModuleOption.DeleteUsers, UserModuleOption.Back)
                    .UseConverter(opt => opt switch
                    {
                        UserModuleOption.ViewUsers => "Ver usuarios actuales",
                        UserModuleOption.RegisterUser => "Registrar usuario nuevo",
                        UserModuleOption.DeleteUsers => "Borrar usuarios",
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
                case UserModuleOption.Back:
                    running = false;
                    break;
            }
        }
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

        string roleChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(Theme.Accent("Rol"))
                .AddChoices(Roles.ADMIN.ToString(), Roles.OPERATOR.ToString(), "Cancelar"));

        if (string.Equals(roleChoice, "Cancelar", StringComparison.OrdinalIgnoreCase))
        {
            AnsiConsole.MarkupLine(Theme.Muted("Registro cancelado."));
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

    private static void Pause()
    {
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine(Theme.Muted("Presiona cualquier tecla para continuar..."));
        Console.ReadKey(true);
    }
}
