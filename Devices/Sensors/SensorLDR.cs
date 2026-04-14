namespace StadiumSystem.Devices.Sensors;

/// <summary>
/// GRASP - Information Expert: conoce y gestiona el nivel de luz captado.
/// </summary>
public class SensorLDR : ISensor
{
    private double _lightLevel;

    public double Read() { return _lightLevel; }
    public void UpdateValue(double value) { }

    /// <summary>Retorna el nivel de luz actual.</summary>
    public double GetLightLevel() { return _lightLevel; }
}
