namespace StadiumSystem.Devices.Actuators;

public interface IActuator
{
    bool IsOn { get; }
    void On();
    void Off();
}
