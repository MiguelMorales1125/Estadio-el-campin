using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StadiumSystem.Controllers;
using StadiumSystem.Domain.Entities;
using StadiumSystem.Infrastructure;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Infrastructure.Security;
using StadiumSystem.UI;

namespace StadiumSystem;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Gemelo Digital - Estadio El Campín";
        using var dbContext = new AppDbContext();
        
        IPasswordHasher passwordHasher = new BcryptPasswordHasher();
        AuthController authController = new AuthController(dbContext, passwordHasher);

        try { authController.SeedAdminIfNotExists(); } catch { }

        var arduino = ArduinoConnection.GetInstance();
        arduino.MessageReceived += OnArduinoMessage;
        arduino.StartListening();

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

        try { arduino.Dispose(); } catch { }
    }

    private static string? _goalFlashText;
    private static DateTime _goalFlashUntil = DateTime.MinValue;
    private static readonly object _arduinoGoalLock = new();
    private static DateTime _lastMovSeenUtc = DateTime.MinValue;
    private static bool _goalAlreadyCountedForThisEvent = false;

    private static string? CurrentFlash()
    {
        if (DateTime.UtcNow < _goalFlashUntil) return _goalFlashText;
        return null;
    }

    private static void OnArduinoMessage(string message)
    {
        if (!string.Equals(message, "MOV", StringComparison.OrdinalIgnoreCase)) return;

        try
        {
            // Ventana de silencio: el sensor debe estar en silencio este tiempo
            // antes de aceptar un nuevo gol. Mientras llegan MOV (aun cada 2s
            // por re-trigger del PIR), la ventana se reinicia y se ignoran.
            int silenceMs = int.TryParse(
                System.Environment.GetEnvironmentVariable("ARDUINO_GOAL_COOLDOWN_MS"),
                out var parsedSilenceMs) ? parsedSilenceMs : 6000;

            bool shouldCount;
            lock (_arduinoGoalLock)
            {
                var now = DateTime.UtcNow;

                // Si pasó suficiente tiempo sin mensajes MOV, se re-arma el
                // detector para aceptar el siguiente evento.
                if ((now - _lastMovSeenUtc).TotalMilliseconds > silenceMs)
                {
                    _goalAlreadyCountedForThisEvent = false;
                }

                _lastMovSeenUtc = now;

                if (_goalAlreadyCountedForThisEvent) return;

                _goalAlreadyCountedForThisEvent = true;
                shouldCount = true;
            }

            if (!shouldCount) return;

            using var db = new AppDbContext();
            var match = db.Matches.FirstOrDefault(m => m.IsActive);
            if (match == null) return;

            string team = (System.Environment.GetEnvironmentVariable("ARDUINO_GOAL_TEAM") ?? "LOCAL").ToUpper();
            string teamName;
            if (team == "AWAY") { match.ScoreAway++; teamName = match.TeamAway; }
            else { match.ScoreLocal++; teamName = match.TeamLocal; }

            db.SaveChanges();

            _goalFlashText = $">> ¡GOL de {teamName}! (Arduino)";
            _goalFlashUntil = DateTime.UtcNow.AddSeconds(2.5);
        }
        catch { }
    }

    private static bool DoLoginFlow(AuthController auth, out User? user)
    {
        user = null;
        var startOptions = new[] { "Iniciar sesión", "Salir" };
        int startSelected = ConsoleHelper.ShowInteractiveMenu("=== Sistema de Gestión - Estadio El Campín ===", "", startOptions);
        if (startSelected == 1 || startSelected == -1) return false;

        Console.Clear();
        Console.WriteLine("[ Ingresar ]");
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
        // 1. CARGAMOS EL ESTADO DIRECTAMENTE DE LA BASE DE DATOS
        var stState = dbContext.StadiumStates.FirstOrDefault(s => s.Id == 1);
        if (stState == null) 
        {
            stState = new StadiumState { Id = 1, Mode = "APAGADO" };
            dbContext.StadiumStates.Add(stState);
            dbContext.SaveChanges();
        }
        string stadiumMode = stState.Mode;

        var activeMatch = dbContext.Matches.FirstOrDefault(m => m.IsActive);
        if (activeMatch != null)
        {
            try { dbContext.Entry(activeMatch).Reload(); } catch { }
        }
        bool matchExists = activeMatch != null;

        // 2. CONSTRUIMOS EL MENÚ ACORDE A SI HAY PARTIDO EN VIVO
        var options = new List<string> { "Cambiar modo actual" };
        
        if (!matchExists) 
        {
            options.Add("Iniciar Nuevo Partido");
        } 
        else 
        {
            options.Add("Registrar Gol");
            options.Add("Reiniciar Marcador");
            options.Add("Finalizar Partido");
        }

        if (user!.Role == "ADMIN" || user.Role == "ADMINISTRADOR")
            options.Add("[ADMIN] Registrar Nuevo Operario");
        
        options.Add("Cerrar Sesión");
        options.Add("Salir");

        string title = $"=== MENÚ PRINCIPAL ({user.Role}: {user.Username}) ===";

        int sel;
        if (matchExists)
        {
            sel = ConsoleHelper.ShowInteractiveMenu(
                title,
                () => BuildLiveSubtitle(stadiumMode),
                options.ToArray(),
                CurrentFlash);
        }
        else
        {
            string sub = $"ESTADO: {stadiumMode}   |   PARTIDO EN VIVO: NO   |   MARCADOR: NINGUNO 0 - 0 NINGUNO";
            sel = ConsoleHelper.ShowInteractiveMenu(title, sub, options.ToArray());
        }
        Console.Clear();

        if (sel == -1) return true; // Clic a ESC resetea el loop principal

        string selectedOption = options[sel];
        string commandExecuted = string.Empty;

        // 3. RUTEO DE COMANDOS EFICIENTE
        if (selectedOption == "Cambiar modo actual") commandExecuted = HandleModeChange(dbContext, stState);
        else if (selectedOption == "Iniciar Nuevo Partido") commandExecuted = HandleNewMatch(dbContext);
        else if (selectedOption == "Registrar Gol") commandExecuted = HandleGoal(dbContext, activeMatch!);
        else if (selectedOption == "Reiniciar Marcador") commandExecuted = HandleResetMarker(dbContext, activeMatch!);
        else if (selectedOption == "Finalizar Partido") commandExecuted = HandleFinishMatch(dbContext, activeMatch!);
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
            return false;
        }

        // 4. AUDITAR LA ACCIÓN EN BASE DE DATOS MIENTRAS REGRESAMOS
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
            // Retraso para ver el Print success sin presionar Enter (Requerimiento)
            ConsoleHelper.ShowSuccess("");
        }

        return true;
    }

    private static string BuildLiveSubtitle(string stadiumMode)
    {
        try
        {
            using var db = new AppDbContext();
            var m = db.Matches.AsNoTracking().FirstOrDefault(x => x.IsActive);
            if (m == null)
                return $"ESTADO: {stadiumMode}   |   PARTIDO EN VIVO: NO   |   MARCADOR: NINGUNO 0 - 0 NINGUNO";
            return $"ESTADO: {stadiumMode}   |   PARTIDO EN VIVO: SÍ   |   MARCADOR: {m.TeamLocal} {m.ScoreLocal} - {m.ScoreAway} {m.TeamAway}";
        }
        catch
        {
            return $"ESTADO: {stadiumMode}   |   PARTIDO EN VIVO: ?";
        }
    }

    private static string HandleModeChange(AppDbContext db, StadiumState state)
    {
        var modeOptions = new[] { "PARTIDO", "MANTENIMIENTO", "EMERGENCIA" };
        int idx = ConsoleHelper.ShowInteractiveMenu("=== SELECCIONAR MODO ===", "", modeOptions);
        Console.Clear();
        if (idx == -1) return "";

        state.Mode = modeOptions[idx];
        db.SaveChanges(); // Guardamos el cambio en PostgreSQL permanentemente

        Console.WriteLine($"\n>> Modo {state.Mode} guardado en BD y dispositivos activados.");
        return $"MODO_{state.Mode}";
    }

    private static string HandleNewMatch(AppDbContext db)
    {
        Console.WriteLine("=== CONFIGURAR NUEVO PARTIDO ===");
        Console.WriteLine("[ESC] para cancelar.\n");
        Console.Write("Nombre del Equipo Local: ");
        string? nLocal = ConsoleHelper.ReadInputWithEsc();
        if (string.IsNullOrWhiteSpace(nLocal)) return "";

        Console.Write("\nNombre del Equipo Visitante: ");
        string? nAway = ConsoleHelper.ReadInputWithEsc();
        if (string.IsNullOrWhiteSpace(nAway)) return "";

        var match = new MatchSession
        {
            TeamLocal = nLocal,
            TeamAway = nAway,
            ScoreLocal = 0,
            ScoreAway = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        db.Matches.Add(match);
        db.SaveChanges(); // Persistimos en PostgreSQL

        Console.WriteLine($"\n>> Partido Oficial Iniciado: {nLocal} vs {nAway}");
        return $"INICIO_PARTIDO_{nLocal}_VS_{nAway}";
    }

    private static string HandleGoal(AppDbContext db, MatchSession match)
    {
        var goals = new[] { match.TeamLocal, match.TeamAway };
        int idx = ConsoleHelper.ShowInteractiveMenu("=== ANOTAR GOL ===", "", goals);
        Console.Clear();
        
        if (idx == 0) 
        { 
            match.ScoreLocal++; 
            db.SaveChanges();
            Console.WriteLine($"\n>> ¡GOL de {match.TeamLocal}!"); 
            return "GOL_LOCAL"; 
        }
        else if (idx == 1) 
        { 
            match.ScoreAway++; 
            db.SaveChanges();
            Console.WriteLine($"\n>> ¡GOL de {match.TeamAway}!"); 
            return "GOL_VISITANTE"; 
        }

        return "";
    }

    private static string HandleResetMarker(AppDbContext db, MatchSession match)
    {
        match.ScoreLocal = 0;
        match.ScoreAway = 0;
        db.SaveChanges();
        Console.WriteLine("\n>> Marcador reiniciado a 0 - 0 en la BD.");
        return "REINICIAR_MARCADOR";
    }

    private static string HandleFinishMatch(AppDbContext db, MatchSession match)
    {
        match.IsActive = false;
        match.FinishedAt = DateTime.UtcNow;
        db.SaveChanges(); // Persistimos cierre histórico

        Console.WriteLine($"\n>> Partido Finalizado históricamente.");
        Console.WriteLine($">> Resultado Final: {match.TeamLocal} {match.ScoreLocal} - {match.ScoreAway} {match.TeamAway}");
        return "FINALIZAR_PARTIDO";
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
