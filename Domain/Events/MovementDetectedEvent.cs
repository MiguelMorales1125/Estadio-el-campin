namespace StadiumSystem.Domain.Events;

public class MovementDetectedEvent : IEvent
{
    public MovementDetectedEvent(string sensorType)
    {
        SensorType = sensorType;
    }

    public string EventType => "MovementDetected";

    public string SensorType { get; }
}
