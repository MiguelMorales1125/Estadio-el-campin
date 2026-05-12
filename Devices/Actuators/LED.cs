using StadiumSystem.Infrastructure;
using StadiumSystem.Commands;

namespace StadiumSystem.Devices.Actuators;

public class LED : IActuator
{
    public int Pin { get; set; }
    public string Type { get; set; } = "";
    public ArduinoConnection? Connection { get; set; }
    public bool IsOn { get; private set; }

    public void On()
    {
        IsOn = true;
        SendCommand(true);
    }

    public void Off()
    {
        IsOn = false;
        SendCommand(false);
    }

    public async Task BlinkAsync(int times = 1, int delayMs = 500)
    {
        for (int i = 0; i < times; i++)
        {
            On();
            await Task.Delay(delayMs);
            Off();
            await Task.Delay(delayMs);
        }
    }

    private void SendCommand(bool turnOn)
    {
        if (Connection is null) return;

        ICommand cmd = turnOn ? new LedOnCommand(Pin) : new LedOffCommand(Pin);
        Connection.SendCommand(cmd);
    }
}
