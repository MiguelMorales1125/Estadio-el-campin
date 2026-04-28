namespace StadiumSystem.Devices.Actuators;

public class LED : IActuator
{
    public bool IsOn { get; private set; }

    public void On() { }
    public void Off() { }
}
