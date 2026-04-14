namespace StadiumSystem.Devices.Actuators;

/// <summary>
/// GRASP - Polymorphism + Protected Variations: contrato común para todos
/// los actuadores del sistema.
/// </summary>
public interface IActuator
{
    bool IsOn { get; }
    void On();
    void Off();
}
