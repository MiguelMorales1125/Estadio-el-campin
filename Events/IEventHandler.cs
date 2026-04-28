namespace StadiumSystem.Events;

public interface IEventHandler
{
    void Handle(IEvent @event);
}
