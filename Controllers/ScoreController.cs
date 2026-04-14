using StadiumSystem.Domain;
using StadiumSystem.Events;

namespace StadiumSystem.Controllers;

/// <summary>
/// GRASP - Controller: maneja los casos de uso relacionados con el marcador.
/// Delega la lógica de datos al Scoreboard (Low Coupling / High Cohesion).
/// </summary>
public class ScoreController : IEventHandler
{
    private Scoreboard _scoreboard;

    public ScoreController()
    {
        _scoreboard = new Scoreboard();
    }

    /// <summary>Reinicia ambos marcadores a cero.</summary>
    public void ResetScores() { }

    /// <summary>Asigna el Scoreboard que este controlador gestiona.</summary>
    public void SetScoreboard(Scoreboard scoreboard) { }

    /// <summary>Incrementa el marcador del equipo local en 1.</summary>
    public void IncrementLocalScore() { }

    /// <summary>Incrementa el marcador del equipo visitante en 1.</summary>
    public void IncrementAwayScore() { }

    /// <inheritdoc/>
    public void Handle(IEvent @event) { }
}
