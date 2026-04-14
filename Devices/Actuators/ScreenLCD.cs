namespace StadiumSystem.Devices.Actuators;

/// <summary>
/// GRASP - Information Expert: conoce cómo interactuar con la pantalla LCD.
/// Implementa IActuator e IScoreDisplay (doble rol de actuador y display).
/// </summary>
public class ScreenLCD : IActuator, IScoreDisplay
{
    public bool IsOn { get; private set; }

    // IActuator
    public void On() { }
    public void Off() { }

    // IScoreDisplay
    public void ShowScores(int scoreLocal, int scoreAway) { }
    public void ShowTemporaryMessage(string message, int durationMs) { }
    public void ClearDisplay() { }
}
