namespace StadiumSystem.Domain.Events;

public interface IEvent
{
    string EventType { get; }
}
