namespace StadiumSystem.Events;

public sealed class ArduinoMessageReceivedEvent : IEvent
{
    public const string TypeName = "ARDUINO.MESSAGE_RECEIVED";

    public string EventType => TypeName;

    public string Message { get; }

    public ArduinoMessageReceivedEvent(string message)
    {
        Message = message;
    }
}