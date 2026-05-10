using StadiumSystem.Domain;
using StadiumSystem.Domain.Events;
using StadiumSystem.Devices.Sensors;
using StadiumSystem.Infrastructure;

namespace StadiumSystem.Controllers;

public class LightController : IEventHandler
{
    private readonly List<Light> _lights;
    private readonly IDeviceRegistry _deviceRegistry;
    private SensorPIR? _pirSensor;

    public LightController(IDeviceRegistry deviceRegistry)
    {
        _deviceRegistry = deviceRegistry;
        _lights = new List<Light>();
        InitializeLights();
        InitializeSensors();
        StadiumSystem.Infrastructure.Events.EventBus.GetInstance().Subscribe("MovementDetected", e => Handle(e));
    }

    private void InitializeLights()
    {
        var leds = _deviceRegistry.GetAllLeds();
        foreach (var led in leds)
        {
            var light = new Light();
            light.SetLed(led);
            _lights.Add(light);
        }
    }

    private void InitializeSensors()
    {
        foreach (var sensorType in _deviceRegistry.GetAllSensorTypes())
        {
            if (sensorType.ToUpperInvariant().Contains("PIR"))
            {
                var sensor = _deviceRegistry.GetSensorByType(sensorType) as SensorPIR;
                if (sensor != null)
                {
                    sensor.MovementDetected += OnMovementDetected;
                }
            }
        }
    }

    private void OnMovementDetected(object? sender, MovementDetectedEventArgs e)
    {
        BlinkLightsAsync(3, 300).ConfigureAwait(false);
    }

    public void TurnLightsOn()
    {
        foreach (var light in _lights)
        {
            light.On();
        }
    }

    public void TurnLightsOff()
    {
        foreach (var light in _lights)
        {
            light.Off();
        }
    }

    public async Task BlinkLightsAsync(int times = 1, int delayMs = 500)
    {
        var tasks = _lights.Select(light => light.BlinkAsync(times, delayMs)).ToList();
        await Task.WhenAll(tasks);
    }

    public void BlinkLights()
    {
        BlinkLightsAsync(1, 500).ConfigureAwait(false);
    }

    public void EmergencyLights()
    {
        BlinkLightsAsync(5, 200).ConfigureAwait(false);
    }

    public void Handle(IEvent @event)
    {
        if (@event == null) return;
        if (@event.EventType == "MovementDetected" && @event is StadiumSystem.Domain.Events.MovementDetectedEvent m)
        {
            BlinkLightsAsync(3, 300).ConfigureAwait(false);
        }
    }
}
