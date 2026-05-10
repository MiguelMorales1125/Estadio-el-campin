using StadiumSystem.Devices.Sensors;
using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Infrastructure;

public interface IDeviceRegistry
{
    void RegisterLed(LED led);
    void RegisterSensor(ISensor sensor, string type);
    void RegisterActuator(IActuator actuator, string type);
    
    List<LED> GetAllLeds();
    ISensor? GetSensorByType(string type);
    IActuator? GetActuatorByType(string type);
    IEnumerable<string> GetAllSensorTypes();
}

public class DeviceRegistry : IDeviceRegistry
{
    private readonly List<LED> _leds = new();
    private readonly Dictionary<string, ISensor> _sensors = new();
    private readonly Dictionary<string, IActuator> _actuators = new();

    public void RegisterLed(LED led)
    {
        _leds.Add(led);
    }

    public void RegisterSensor(ISensor sensor, string type)
    {
        _sensors[type] = sensor;
    }

    public void RegisterActuator(IActuator actuator, string type)
    {
        _actuators[type] = actuator;
    }

    public List<LED> GetAllLeds() => _leds;

    public ISensor? GetSensorByType(string type)
    {
        _sensors.TryGetValue(type, out var sensor);
        return sensor;
    }

    public IActuator? GetActuatorByType(string type)
    {
        _actuators.TryGetValue(type, out var actuator);
        return actuator;
    }

    public IEnumerable<string> GetAllSensorTypes() => _sensors.Keys;
}
