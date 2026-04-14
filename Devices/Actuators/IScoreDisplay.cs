namespace StadiumSystem.Devices.Actuators;

/// <summary>
/// GRASP - Protected Variations: abstrae la visualización del marcador,
/// permitiendo cambiar la pantalla sin afectar al Scoreboard.
/// </summary>
public interface IScoreDisplay
{
    void ShowScores(int scoreLocal, int scoreAway);
    void ShowTemporaryMessage(string message, int durationMs);
    void ClearDisplay();
}
