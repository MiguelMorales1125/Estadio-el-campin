namespace StadiumSystem.Devices.Actuators;

/// <summary>
/// GRASP - Information Expert: conoce su propio estado de encendido/apagado.
/// </summary>
public class LED : IActuator
{
    public bool IsOn { get; private set; }

    public void On() { }
    public void Off() { }
}
