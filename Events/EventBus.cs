namespace StadiumSystem.Events;

public class EventBus
{
    private readonly Dictionary<string, List<Action<IEvent>>> _subscribers;
    private static EventBus? _instance;

    private EventBus()
    {
        _subscribers = new Dictionary<string, List<Action<IEvent>>>();
    }

    public static EventBus GetInstance()
    {
        _instance ??= new EventBus();
        return _instance;
    }

    public void Publish(IEvent @event) { }

    public void Subscribe(string eventType, Action<IEvent> handler) { }

    public void Unsubscribe(string eventType, Action<IEvent> handler) { }
}
