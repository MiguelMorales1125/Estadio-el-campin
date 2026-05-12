using StadiumSystem.Infrastructure;
using StadiumSystem.Devices.Sensors;
using StadiumSystem.Devices.Actuators;
using StadiumSystem.Services;

namespace StadiumSystem.Devices;

public class DeviceFactory
{
    private readonly ITerminalLogService? _logService;
    private readonly ArduinoConnection? _connection;

    public DeviceFactory(ITerminalLogService? logService = null, ArduinoConnection? connection = null)
    {
        _logService = logService;
        _connection = connection;
    }

    public LED? CreateLed(int pin, string type)
    {
        if (pin < 0)
            return null;

        return new LED { Pin = pin, Type = type, Connection = _connection };
    }

    public ISensor? CreateSensor(string type, int pin)
    {
        if (pin < 0)
            return null;

        ISensor? sensor = type.ToUpperInvariant() switch
        {
            "PIR" => new SensorPIR { Pin = pin, SensorType = "PIR" },
            "PIR_HOME" => new SensorPIR { Pin = pin, SensorType = "PIR_HOME" },
            "PIR_AWAY" => new SensorPIR { Pin = pin, SensorType = "PIR_AWAY" },
            "LDR" => new SensorLDR { Pin = pin },
            _ => null
        };

        if (sensor is SensorPIR pir)
        {
            pir.LogService = _logService;
        }

        return sensor;
    }

    public IActuator? CreateActuator(string type, int pin)
    {
        if (pin < 0)
            return null;

        return type.ToUpperInvariant() switch
        {
            "BUZZER" => new Buzzer { Pin = pin },
            "LED" => new LED { Pin = pin, Type = type, Connection = _connection },
            "LCD" => new ScreenLCD(_connection) { },
            _ => null
        };
    }
}
