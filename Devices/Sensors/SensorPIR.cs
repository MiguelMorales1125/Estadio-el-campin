namespace StadiumSystem.Devices.Sensors;

public class SensorPIR : ISensor
{
    private bool _detected;

    public double Read() { return default; }
    public void UpdateValue(double value) { }
    public bool IsDetected() { return _detected; }
}
