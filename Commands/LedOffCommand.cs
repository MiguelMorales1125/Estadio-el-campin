namespace StadiumSystem.Commands;

public class LedOffCommand : ICommand
{
    public int Pin { get; }

    public LedOffCommand(int pin)
    {
        Pin = pin;
    }

    public string Serialize() => $"LED_OFF:{Pin}";
}
