namespace StadiumSystem.Devices.Sensors;

public class SensorLDR : ISensor
{
    private double _lightLevel;

    public double Read() { return _lightLevel; }
    public void UpdateValue(double value) { }
    public double GetLightLevel() { return _lightLevel; }
}
