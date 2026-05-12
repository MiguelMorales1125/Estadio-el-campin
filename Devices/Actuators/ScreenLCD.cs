using StadiumSystem.Commands;
using StadiumSystem.Infrastructure;

namespace StadiumSystem.Devices.Actuators;

public class ScreenLCD : IActuator, IScoreDisplay
{
    private readonly ArduinoConnection? _connection;
    public bool IsOn { get; private set; }

    public ScreenLCD(ArduinoConnection? connection = null)
    {
        _connection = connection;
    }

    public void On() => IsOn = true;
    public void Off() => IsOn = false;

    public void ShowScores(int scoreLocal, int scoreAway)
    {
        IsOn = true;
        if (_connection is not null)
        {
            _connection.SendCommand(new LcdShowScoresCommand(scoreLocal, scoreAway));
        }
    }
}
