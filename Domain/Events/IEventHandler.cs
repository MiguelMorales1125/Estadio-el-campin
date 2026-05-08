namespace StadiumSystem.Domain.Events;

public interface IEventHandler
{
    void Handle(IEvent @event);
}
