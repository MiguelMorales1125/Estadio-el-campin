namespace StadiumSystem.Devices.Sensors;


public interface ISensor
{
    double Read();
    void UpdateValue(double value);
}
