namespace StadiumSystem.Events;

/// <summary>
/// GRASP - Polymorphism + Protected Variations: desacopla emisores de
/// receptores mediante un contrato de manejo de eventos.
/// </summary>
public interface IEventHandler
{
    void Handle(IEvent @event);
}
