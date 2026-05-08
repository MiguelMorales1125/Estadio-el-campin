using StadiumSystem.Domain.Events;

namespace StadiumSystem.Infrastructure.Events;

public class EventBus
{
    private readonly Dictionary<string, List<Action<IEvent>>> _subscribers;
    private readonly object _lock = new();
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

    public void Publish(IEvent @event)
    {
        List<Action<IEvent>> handlers;

        lock (_lock)
        {
            if (!_subscribers.TryGetValue(@event.EventType, out var registeredHandlers)) return;
            handlers = new List<Action<IEvent>>(registeredHandlers);
        }

        foreach (var handler in handlers)
        {
            try
            {
                handler(@event);
            }
            catch
            {
            }
        }
    }

    public void Subscribe(string eventType, Action<IEvent> handler)
    {
        lock (_lock)
        {
            if (!_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Action<IEvent>>();
                _subscribers[eventType] = handlers;
            }

            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }
    }

    public void Unsubscribe(string eventType, Action<IEvent> handler)
    {
        lock (_lock)
        {
            if (!_subscribers.TryGetValue(eventType, out var handlers)) return;

            handlers.Remove(handler);

            if (handlers.Count == 0)
            {
                _subscribers.Remove(eventType);
            }
        }
    }
}
