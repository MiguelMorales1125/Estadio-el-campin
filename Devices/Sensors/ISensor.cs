namespace StadiumSystem.Devices.Sensors;

/// <summary>
/// GRASP - Polymorphism + Protected Variations: contrato común para todos
/// los sensores, permitiendo agregar nuevos tipos sin romper el sistema.
/// </summary>
public interface ISensor
{
    double Read();
    void UpdateValue(double value);
}
