using StadiumSystem.Infrastructure;

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
        SendState(true);
    }

    public void Off()
    {
        IsOn = false;
        SendState(false);
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

    private void SendState(bool isOn)
    {
        if (Connection is null)
        {
            return;
        }

        Connection.SendRawCommand($"LED_{(isOn ? "ON" : "OFF")}:{Pin}");
    }
}
