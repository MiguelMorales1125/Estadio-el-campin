namespace StadiumSystem.Events;

/// <summary>
/// GRASP - Controller + Creator: coordina la publicación y suscripción de
/// eventos en el sistema. Singleton garantiza una única instancia global.
/// </summary>
public class EventBus
{
    // GRASP - Information Expert: conoce y gestiona los suscriptores del sistema.
    private readonly Dictionary<string, List<Action<IEvent>>> _subscribers;
    private static EventBus? _instance;

    private EventBus()
    {
        _subscribers = new Dictionary<string, List<Action<IEvent>>>();
    }

    /// <summary>Singleton: retorna la única instancia del bus.</summary>
    public static EventBus GetInstance()
    {
        _instance ??= new EventBus();
        return _instance;
    }

    /// <summary>Publica un evento a todos los suscriptores registrados.</summary>
    public void Publish(IEvent @event) { }

    /// <summary>Registra un handler para un tipo de evento específico.</summary>
    public void Subscribe(string eventType, Action<IEvent> handler) { }

    /// <summary>Elimina un handler previamente registrado.</summary>
    public void Unsubscribe(string eventType, Action<IEvent> handler) { }
}
