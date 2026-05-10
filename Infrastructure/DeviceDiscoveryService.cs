using StadiumSystem.Devices;
using StadiumSystem.Domain;

namespace StadiumSystem.Infrastructure;

public interface IDeviceDiscoveryService
{
    Task<bool> DiscoverDevicesAsync();
}

public class DeviceDiscoveryService : IDeviceDiscoveryService
{
    private readonly ArduinoConnection _arduinoConnection;
    private readonly DeviceFactory _deviceFactory;
    private readonly IDeviceRegistry _deviceRegistry;
    private readonly ArduinoInventoryParser _parser;
    private bool _inventoryReceived = false;
    private string _inventoryMessage = "";
    private readonly object _lock = new();

    public DeviceDiscoveryService(
        ArduinoConnection arduinoConnection,
        DeviceFactory deviceFactory,
        IDeviceRegistry deviceRegistry)
    {
        _arduinoConnection = arduinoConnection;
        _deviceFactory = deviceFactory;
        _deviceRegistry = deviceRegistry;
        _parser = new ArduinoInventoryParser();
    }

    public async Task<bool> DiscoverDevicesAsync()
    {
        _inventoryReceived = false;
        _inventoryMessage = "";

        Action<string> handler = OnInventoryReceived;
        _arduinoConnection.MessageReceived += handler;

        try
        {
            Console.WriteLine("[Discovery] Solicitando inventario a Arduino, esperando respuesta...");
            var start = DateTime.UtcNow;
            var maxWait = TimeSpan.FromSeconds(30);
            while (!_inventoryReceived && DateTime.UtcNow - start < maxWait)
            {
                _arduinoConnection.RequestInventory();
                await Task.Delay(2000);
                if (!_inventoryReceived)
                    Console.WriteLine("[Discovery] Esperando inventario... (enviando solicitud)");
            }

            if (!_inventoryReceived)
            {
                Console.WriteLine("[Discovery] No se recibió inventario en el tiempo esperado");
                return false;
            }

            var inventory = _parser.Parse(_inventoryMessage);
            CreateDevicesFromInventory(inventory);

            return true;
        }
        finally
        {
            _arduinoConnection.MessageReceived -= handler;
        }
    }

    private void OnInventoryReceived(string message)
    {
        if (message.StartsWith("INVENTORY:"))
        {
            lock (_lock)
            {
                _inventoryMessage = message;
                _inventoryReceived = true;
            }
        }
    }

    private void CreateDevicesFromInventory(ArduinoInventoryMessage inventory)
    {
        foreach (var ledInfo in inventory.Leds)
        {
            var led = _deviceFactory.CreateLed(ledInfo.Pin, ledInfo.Type);
            if (led != null)
                _deviceRegistry.RegisterLed(led);
        }

        foreach (var sensorInfo in inventory.Sensors)
        {
            var sensor = _deviceFactory.CreateSensor(sensorInfo.Type, sensorInfo.Pin);
            if (sensor != null)
                _deviceRegistry.RegisterSensor(sensor, sensorInfo.Type);
        }

        foreach (var actuatorInfo in inventory.Actuators)
        {
            var actuator = _deviceFactory.CreateActuator(actuatorInfo.Type, actuatorInfo.Pin);
            if (actuator != null)
                _deviceRegistry.RegisterActuator(actuator, actuatorInfo.Type);
        }
    }
}
