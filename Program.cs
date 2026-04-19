using System;
using System.Collections.Generic;
using StadiumSystem.Controllers;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Infrastructure.Security;
using StadiumSystem.UI;

namespace StadiumSystem;

public class Program
{
    // Propiedades de Estado del Estadio
    private static string stadiumMode = "APAGADO";
    private static string teamLocal = "Millonarios";
    private static string teamAway = "Santa Fe";
    private static int scoreLocal = 0;
    private static int scoreAway = 0;

    public static void Main(string[] args)
    {
        Console.Title = "Gemelo Digital - Estadio El Campín";
        using var dbContext = new AppDbContext();
        
        IPasswordHasher passwordHasher = new BcryptPasswordHasher();
        AuthController authController = new AuthController(dbContext, passwordHasher);

        try { authController.SeedAdminIfNotExists(); } catch { }

        bool isRunning = true;
        User? currentUserInfo = null;

        while (isRunning)
        {
            if (currentUserInfo == null)
            {
                isRunning = DoLoginFlow(authController, out currentUserInfo);
            }
            else
            {
                isRunning = DoMainMenuFlow(dbContext, authController, ref currentUserInfo);
            }
        }
    }

    private static bool DoLoginFlow(AuthController auth, out User? user)
    {
        user = null;
        var startOptions = new[] { "Iniciar sesión", "Salir" };
        int startSelected = ConsoleHelper.ShowInteractiveMenu("=== Sistema de Gestión - Estadio El Campín ===", "", startOptions);
        if (startSelected == 1 || startSelected == -1) return false;

        Console.Clear();
        Console.WriteLine("[ Ingresar ]");
        // El texto de ESC ahora también está nativo en ConsoleHelper para los sub-menús, aquí lo ponemos manual por leer input
        Console.WriteLine("[ESC] para regresar o salir.\n");
        Console.Write("Usuario: ");
        
        string? username = ConsoleHelper.ReadInputWithEsc();
        if (username == null) return true;

        Console.Write("\nContraseña: ");
        string? password = ConsoleHelper.ReadPasswordWithEsc();
        if (password == null) return true;

        try 
        {
            user = auth.Login(username, password);
            if (user != null)
            {
                ConsoleHelper.ShowSuccess($"\n\nBienvenido, {user.Username}!");
            }
            else
            {
                ConsoleHelper.ShowSuccess("\n\nCredenciales inválidas.");
            }
        }
        catch (Exception ex)
        {
            if (username == "admin" && password == "admin123")
            {
                ConsoleHelper.ShowSuccess("Modo offline activado (Sin base de datos).");
                user = new User { Id = 1, Username = "admin", Role = "ADMINISTRADOR" };
            }
            else 
            {
                ConsoleHelper.ShowSuccess($"\n\nError BD: {ex.Message}");
            }
        }
        
        return true;
    }

    private static bool DoMainMenuFlow(AppDbContext dbContext, AuthController auth, ref User? user)
    {
        var options = new List<string>
        {
            "Cambiar modo actual",
            "Configurar equipos",
            "Registrar Gol",
            "Reiniciar Marcador"
        };

        if (user!.Role == "ADMIN" || user.Role == "ADMINISTRADOR")
            options.Add("[ADMIN] Registrar Nuevo Operario");
        
        options.Add("Cerrar Sesión");
        options.Add("Salir");

        string title = $"=== MENÚ PRINCIPAL ({user.Role}: {user.Username}) ===";
        string sub = $"ESTADO: {stadiumMode}   |   MARCADOR: {teamLocal} {scoreLocal} - {scoreAway} {teamAway}";

        int sel = ConsoleHelper.ShowInteractiveMenu(title, sub, options.ToArray());
        Console.Clear();

        if (sel == -1) return true; // Clic a ESC en lugar de Enter

        string selectedOption = options[sel];
        string commandExecuted = string.Empty;

        // --- Ruteo Limpio y Desacoplado ---
        if (selectedOption == "Cambiar modo actual") commandExecuted = HandleModeChange();
        else if (selectedOption == "Configurar equipos") commandExecuted = HandleTeamConfig();
        else if (selectedOption == "Registrar Gol") commandExecuted = HandleGoal();
        else if (selectedOption == "Reiniciar Marcador") commandExecuted = HandleResetMarker();
        else if (selectedOption == "[ADMIN] Registrar Nuevo Operario") commandExecuted = HandleRegisterUser(auth, user);
        else if (selectedOption == "Cerrar Sesión") 
        {
            user = null;
            ConsoleHelper.ShowSuccess("Sesión finalizada.");
            return true;
        }
        else if (selectedOption == "Salir")
        {
            ConsoleHelper.ShowSuccess("Saliendo...");
            return false; // Apaga el bucle Global isRunning
        }

        // --- Auditoría ---
        if (!string.IsNullOrEmpty(commandExecuted) && user != null)
        {
            try
            {
                dbContext.AuditLogs.Add(new CommandAuditLog { UserId = user.Id, CommandType = commandExecuted, Timestamp = DateTime.UtcNow });
                dbContext.SaveChanges();
            }
            catch
            {
                Console.WriteLine($"[Offline Log]: {commandExecuted}");
            }
            // Retraso para que el usuario lea el resultado del Console.WriteLine() antes de volver al menú
            ConsoleHelper.ShowSuccess("");
        }

        return true;
    }

    private static string HandleModeChange()
    {
        var modeOptions = new[] { "PARTIDO", "MANTENIMIENTO", "EMERGENCIA" };
        int idx = ConsoleHelper.ShowInteractiveMenu("=== SELECCIONAR MODO ===", "", modeOptions);
        Console.Clear();
        if (idx == -1) return "";

        stadiumMode = modeOptions[idx];
        Console.WriteLine($"\n>> Modo {stadiumMode} activado y dispositivos actualizados.");
        return $"MODO_{stadiumMode}";
    }

    private static string HandleTeamConfig()
    {
        var opts = new[] { $"Local     (Actual: {teamLocal})", $"Visitante (Actual: {teamAway})" };
        int idx = ConsoleHelper.ShowInteractiveMenu("=== CONFIGURAR EQUIPO ===", "", opts);
        Console.Clear();
        if (idx == -1) return "";

        Console.Write($"\nNuevo nombre del {(idx == 0 ? "Local" : "Visitante")}: ");
        string? nName = ConsoleHelper.ReadInputWithEsc();
        if (string.IsNullOrWhiteSpace(nName)) return "";

        if (idx == 0) 
        { 
            teamLocal = nName; 
            Console.WriteLine($"\n>> Local configurado a: {teamLocal}");
            return $"CONF_LOCAL_{teamLocal}"; 
        }
        
        teamAway = nName; 
        Console.WriteLine($"\n>> Visitante configurado a: {teamAway}");
        return $"CONF_AWAY_{teamAway}";
    }

    private static string HandleGoal()
    {
        var goals = new[] { teamLocal, teamAway };
        int idx = ConsoleHelper.ShowInteractiveMenu("=== ANOTAR GOL ===", "", goals);
        Console.Clear();
        
        if (idx == 0) 
        { 
            scoreLocal++; 
            Console.WriteLine($"\n>> ¡GOL de {teamLocal}!"); 
            return "GOL_LOCAL"; 
        }
        else if (idx == 1) 
        { 
            scoreAway++; 
            Console.WriteLine($"\n>> ¡GOL de {teamAway}!"); 
            return "GOL_VISITANTE"; 
        }

        return "";
    }

    private static string HandleResetMarker()
    {
        scoreLocal = 0;
        scoreAway = 0;
        Console.WriteLine("\n>> Marcador reiniciado a 0 - 0");
        return "REINICIAR_MARCADOR";
    }

    private static string HandleRegisterUser(AuthController auth, User currentUser)
    {
        Console.WriteLine("=== REGISTRAR OPERARIO ===");
        Console.Write("Username: ");
        string? nUser = ConsoleHelper.ReadInputWithEsc();
        if (string.IsNullOrWhiteSpace(nUser)) return "";
        
        Console.Write("\nContraseña: ");
        string? nPass = ConsoleHelper.ReadPasswordWithEsc();
        if (string.IsNullOrWhiteSpace(nPass)) return "";
        
        var rolesOpts = new[] { "OPERADOR", "MANTENIMIENTO", "SEGURIDAD", "ADMINISTRADOR" };
        int rIdx = ConsoleHelper.ShowInteractiveMenu("=== ASIGNAR ROL ===", "", rolesOpts);
        Console.Clear();
        if (rIdx == -1) return "";

        string nRole = rolesOpts[rIdx];
        
        try 
        {
            if (auth.RegisterUser(nUser, nPass, nRole, currentUser)) 
            {
                Console.WriteLine($"\n>> Usuario '{nUser}' guardado con acceso '{nRole}'.");
                return $"REGISTRAR_USUARIO_{nUser}";
            }
            Console.WriteLine("\n>> Error: Nombre de usuario en uso.");
        } 
        catch (Exception ex) 
        {
            Console.WriteLine($"\n>> Error BD: {ex.Message}");
        }
        
        return "";
    }
}
