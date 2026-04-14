using StadiumSystem.Devices.Sensors;
using StadiumSystem.Devices.Actuators;

namespace StadiumSystem.Devices;

/// <summary>
/// GRASP - Creator + Pure Fabrication: centraliza la creación de sensores y
/// actuadores, evitando que otros objetos dependan de clases concretas.
/// </summary>
public class DeviceFactory
{
    public DeviceFactory() { }

    /// <summary>
    /// Crea un sensor del tipo indicado asociado al pin especificado.
    /// </summary>
    public ISensor CreateSensor(string type, string pin) { return null!; }

    /// <summary>
    /// Crea un actuador del tipo indicado asociado al pin especificado.
    /// </summary>
    public IActuator CreateActuator(string type, string pin) { return null!; }
}
