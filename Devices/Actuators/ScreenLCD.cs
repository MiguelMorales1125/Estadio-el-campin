namespace StadiumSystem.Devices.Actuators;

public class ScreenLCD : IActuator, IScoreDisplay
{
    public bool IsOn { get; private set; }
    public void On() { }
    public void Off() { }

    public void ShowScores(int scoreLocal, int scoreAway) { }
    public void ShowTemporaryMessage(string message, int durationMs) { }
    public void ClearDisplay() { }
}
