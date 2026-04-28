using StadiumSystem.Devices.Sensors;
using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Devices;


public class DeviceFactory
{
    public DeviceFactory() { }

    public ISensor CreateSensor(string type, string pin) { return null!; }

    public IActuator CreateActuator(string type, string pin) { return null!; }
}
