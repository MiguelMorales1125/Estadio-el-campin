using StadiumSystem.Domain.Events;
using StadiumSystem.Services;

namespace StadiumSystem.Devices.Sensors;

public class SensorPIR : ISensor
{
    private bool _detected;
    public int Pin { get; set; }
    public string SensorType { get; set; } = "PIR";

    public ITerminalLogService? LogService { get; set; }

    public event EventHandler<MovementDetectedEventArgs>? MovementDetected;

    public double Read()
    {
        return _detected ? 1.0 : 0.0;
    }

    public void UpdateValue(double value)
    {
        bool newState = value > 0.5;
        LogService?.Add(LogLevel.Debug, $"SensorPIR UpdateValue({value}), newState={newState}, _detected={_detected}");

        if (newState && !_detected)
        {
            _detected = true;
            LogService?.Add(LogLevel.Info, $"SensorPIR Disparando OnMovementDetected for {SensorType}");
            OnMovementDetected();
        }
        else if (!newState && _detected)
        {
            _detected = false;
            LogService?.Add(LogLevel.Debug, $"SensorPIR Movimiento terminado for {SensorType}");
        }
    }

    public bool IsDetected() => _detected;

    protected virtual void OnMovementDetected()
    {
        MovementDetected?.Invoke(this, new MovementDetectedEventArgs());
        try
        {
            LogService?.Add(LogLevel.Info, $"SensorPIR publicando MovementDetectedEvent for {SensorType}");
            StadiumSystem.Infrastructure.Events.EventBus.GetInstance().Publish(new StadiumSystem.Domain.Events.MovementDetectedEvent(SensorType));
        }
        catch (Exception ex)
        {
            LogService?.Add(LogLevel.Error, $"Error publicando MovementDetectedEvent: {ex.Message}");
        }
    }
}
