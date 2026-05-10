using StadiumSystem.Devices;
using StadiumSystem.Domain;
using StadiumSystem.Services;

namespace StadiumSystem.Infrastructure;

public class ArduinoRuntimeProcessor : IDisposable
{
    private readonly ArduinoConnection _connection;
    private readonly IDeviceRegistry _registry;
    private readonly DeviceFactory _factory;
    private readonly StadiumSystem.Services.ITerminalLogService? _logService;

    public ArduinoRuntimeProcessor(ArduinoConnection connection, IDeviceRegistry registry, DeviceFactory factory, StadiumSystem.Services.ITerminalLogService? logService = null)
    {
        _connection = connection;
        _registry = registry;
        _factory = factory;
        _logService = logService;
        _connection.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(string message)
    {
        try
        {
            _logService?.Add(LogLevel.Debug, $"Mensaje recibido: {message}");
            if (string.IsNullOrWhiteSpace(message)) return;
            if (message.StartsWith("INVENTORY:"))
            {
                var parser = new ArduinoInventoryParser();
                var inventory = parser.Parse(message);
                CreateDevicesFromInventory(inventory);
                return;
            }

            if (message.StartsWith("SENSOR_TRIGGER:"))
            {
                var parts = message.Split(':');
                if (parts.Length >= 2)
                {
                    var sensorType = parts[1].Trim();
                    _logService?.Add(LogLevel.Debug, $"Buscando sensor: {sensorType}");
                    var sensor = _registry.GetSensorByType(sensorType);
                    if (sensor is Devices.Sensors.SensorPIR pir)
                    {
                        _logService?.Add(LogLevel.Info, $"Sensor encontrado: {sensorType}");
                        pir.UpdateValue(1.0);
                        Task.Delay(300).Wait();
                        pir.UpdateValue(0.0);
                    }
                    else
                    {
                        _logService?.Add(LogLevel.Warn, $"Sensor no encontrado o no es PIR: {sensor?.GetType().Name ?? "null"}");
                    }
                }
                return;
            }

            if (message.StartsWith("SENSOR_UPDATE:"))
            {
                var parts = message.Split(':');
                if (parts.Length >= 3)
                {
                    var sensorType = parts[1].Trim();
                    if (double.TryParse(parts[2].Trim(), out var val))
                    {
                        var sensor = _registry.GetSensorByType(sensorType);
                        sensor?.UpdateValue(val);
                    }
                }
                return;
            }
        }
        catch { }
    }

    private void CreateDevicesFromInventory(ArduinoInventoryMessage inventory)
    {
        foreach (var ledInfo in inventory.Leds)
        {
            var led = _factory.CreateLed(ledInfo.Pin, ledInfo.Type);
            if (led != null)
                _registry.RegisterLed(led);
        }

        foreach (var sensorInfo in inventory.Sensors)
        {
            var sensor = _factory.CreateSensor(sensorInfo.Type, sensorInfo.Pin);
            if (sensor != null)
                _registry.RegisterSensor(sensor, sensorInfo.Type);
        }

        foreach (var actuatorInfo in inventory.Actuators)
        {
            var actuator = _factory.CreateActuator(actuatorInfo.Type, actuatorInfo.Pin);
            if (actuator != null)
                _registry.RegisterActuator(actuator, actuatorInfo.Type);
        }
    }

    public void Dispose()
    {
        try { _connection.MessageReceived -= OnMessageReceived; } catch { }
    }
}
