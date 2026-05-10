using System.Linq;
using StadiumSystem.Domain;
using StadiumSystem.Domain.Events;
using StadiumSystem.Infrastructure.Data;
using StadiumSystem.Services;

namespace StadiumSystem.Controllers;

public class ScoreController : IEventHandler
{
    private Scoreboard _scoreboard;
    private readonly ITerminalLogService? _logService;

    public ScoreController(ITerminalLogService? logService = null)
    {
        _scoreboard = new Scoreboard();
        _logService = logService;
    }

    public void ResetScores()
    {
        _scoreboard = new Scoreboard();
    }

    public void SetScoreboard(Scoreboard scoreboard)
    {
        _scoreboard = scoreboard;
    }

    public void IncrementLocalScore()
    {
        _scoreboard.IncreaseLocal();
        var (l, a) = _scoreboard.GetScores();
        _logService?.Add(LogLevel.Info, $"Local scored. Score {l} - {a}");

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
        if (@event == null) return;
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
