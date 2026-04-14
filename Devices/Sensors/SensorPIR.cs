namespace StadiumSystem.Devices.Sensors;

/// <summary>
/// GRASP - Information Expert: conoce y gestiona el estado de detección PIR.
/// </summary>
public class SensorPIR : ISensor
{
    private bool _detected;

    public double Read() { return default; }
    public void UpdateValue(double value) { }

    /// <summary>Indica si se detectó movimiento.</summary>
    public bool IsDetected() { return _detected; }
}
