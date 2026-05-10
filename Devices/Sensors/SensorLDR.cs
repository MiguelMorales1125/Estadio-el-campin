namespace StadiumSystem.Devices.Sensors;

public class SensorLDR : ISensor
{
    private double _lightLevel;
    public int Pin { get; set; }

    public double Read() => _lightLevel;

    public void UpdateValue(double value)
    {
        _lightLevel = value;
    }

    public double GetLightLevel() => _lightLevel;
}
