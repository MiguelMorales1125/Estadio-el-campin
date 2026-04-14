using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Domain;

/// <summary>
/// GRASP - Information Expert: es el experto en los marcadores del partido
/// y delega la visualización a IScoreDisplay (Low Coupling).
/// </summary>
public class Scoreboard
{
    private int _scoreLocal;
    private int _scoreAway;
    private IScoreDisplay _display;

    /// <summary>Actualiza el marcador del equipo indicado.</summary>
    public void SetScore(string team, int newScore) { }

    /// <summary>Retorna los marcadores actuales como tupla.</summary>
    public (int local, int away) GetScores() { return (_scoreLocal, _scoreAway); }

    /// <summary>Asigna o reemplaza el display de visualización.</summary>
    public void SetDisplay(IScoreDisplay display) { }
}
