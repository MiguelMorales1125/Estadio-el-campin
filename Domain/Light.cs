using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Domain;

public class Light
{
    private LED _led;

    public void On() { }
    public void Off() { }
    public bool IsOn() { return false; }
    public void SetLed(LED led) { }
}
