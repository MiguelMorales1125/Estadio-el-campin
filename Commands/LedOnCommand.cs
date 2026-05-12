namespace StadiumSystem.Commands;

public class LedOnCommand : ICommand
{
    public int Pin { get; }

    public LedOnCommand(int pin)
    {
        Pin = pin;
    }

    public string Serialize() => $"LED_ON:{Pin}";
}
