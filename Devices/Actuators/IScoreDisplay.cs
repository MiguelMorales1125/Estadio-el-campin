namespace StadiumSystem.Devices.Actuators;

public interface IScoreDisplay
{
    void ShowScores(int scoreLocal, int scoreAway);
    void ShowTemporaryMessage(string message, int durationMs);
    void ClearDisplay();
}
