namespace StadiumSystem.Events;

/// <summary>
/// GRASP - Polymorphism: contrato común para todos los eventos del sistema.
/// </summary>
public interface IEvent
{
    string GetType();
}
