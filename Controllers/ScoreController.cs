using System.Linq;
using StadiumSystem.Commands;
using StadiumSystem.Domain;
using StadiumSystem.Domain.Events;
using StadiumSystem.Infrastructure;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Services;

namespace StadiumSystem.Controllers;

public class ScoreController : IEventHandler
{
    private readonly IDeviceRegistry _deviceRegistry;
    private readonly ArduinoConnection _arduinoConnection;
    private readonly ITerminalLogService? _logService;
    private Scoreboard _scoreboard;

    public ScoreController(
        IDeviceRegistry deviceRegistry,
        ArduinoConnection arduinoConnection,
        ITerminalLogService? logService = null)
    {
        _deviceRegistry = deviceRegistry;
        _arduinoConnection = arduinoConnection;
        _logService = logService;
        _scoreboard = new Scoreboard();
    }

    public void ResetScores()
    {
        _scoreboard = new Scoreboard();
    }

    public void SetScoreboard(Scoreboard scoreboard)
    {
        _scoreboard = scoreboard;
    }

    public (int local, int away) GetScores() => _scoreboard.GetScores();

    private void BlinkAllLeds()
    {
        var leds = _deviceRegistry.GetAllLeds();
        foreach (var led in leds)
        {
            _arduinoConnection.SendCommand(new LedOnCommand(led.Pin));
        }
    }

    private void UpdateScoreboardDisplay()
    {
        var (local, away) = _scoreboard.GetScores();
        var lcd = _deviceRegistry.GetActuatorByType("LCD") as Devices.Actuators.ScreenLCD;
        lcd?.ShowScores(local, away);
    }

    private async Task BlinkLedsWithDelayAsync(int times, int delayMs)
    {
        var leds = _deviceRegistry.GetAllLeds();
        for (int i = 0; i < times; i++)
        {
            foreach (var led in leds)
            {
                _arduinoConnection.SendCommand(new LedOnCommand(led.Pin));
            }
            await Task.Delay(delayMs);
            foreach (var led in leds)
            {
                _arduinoConnection.SendCommand(new LedOffCommand(led.Pin));
            }
            await Task.Delay(delayMs);
        }
    }

    public void IncrementLocalScore()
    {
        _scoreboard.IncreaseLocal();
        var (l, a) = _scoreboard.GetScores();
        _logService?.Add(LogLevel.Info, $"Local scored. Score {l} - {a}");

        _ = BlinkLedsWithDelayAsync(3, 300);
        UpdateScoreboardDisplay();

        try
        {
            using var db = new AppDbContext();
            var match = db.Matches.FirstOrDefault(m => m.IsActive);
            if (match != null)
            {
                match.ScoreLocal += 1;
                db.SaveChanges();
                _logService?.Add(LogLevel.Info, $"DB actualizado: Match {match.Id} -> {match.ScoreLocal} - {match.ScoreAway}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Score] Error actualizando DB: {ex.Message}");
        }
    }

    public void IncrementAwayScore()
    {
        _scoreboard.IncreaseAway();
        var (l, a) = _scoreboard.GetScores();
        _logService?.Add(LogLevel.Info, $"Away scored. Score {l} - {a}");

        _ = BlinkLedsWithDelayAsync(3, 300);
        UpdateScoreboardDisplay();

        try
        {
            using var db = new AppDbContext();
            var match = db.Matches.FirstOrDefault(m => m.IsActive);
            if (match != null)
            {
                match.ScoreAway += 1;
                db.SaveChanges();
                _logService?.Add(LogLevel.Info, $"DB actualizado: Match {match.Id} -> {match.ScoreLocal} - {match.ScoreAway}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Score] Error actualizando DB: {ex.Message}");
        }
    }

    public void Handle(IEvent @event)
    {
        if (@event is null) return;
        _logService?.Add(LogLevel.Debug, $"Evento recibido: {@event.EventType}");
        if (@event.EventType == "MovementDetected" && @event is MovementDetectedEvent m)
        {
            var sensor = m.SensorType.ToUpperInvariant();
            _logService?.Add(LogLevel.Info, $"MovementDetected de {sensor}");
            if (sensor == "PIR_HOME")
            {
                IncrementLocalScore();
            }
            else if (sensor == "PIR_AWAY")
            {
                IncrementAwayScore();
            }
        }
    }
}
