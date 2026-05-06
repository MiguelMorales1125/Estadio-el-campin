using StadiumSystem.Events;

namespace StadiumSystem.Infrastructure;

public sealed class ArduinoMessageBusAdapter : IDisposable
{
    private readonly ArduinoConnection _arduinoConnection;
    private readonly EventBus _eventBus;

    public ArduinoMessageBusAdapter(ArduinoConnection arduinoConnection, EventBus eventBus)
    {
        _arduinoConnection = arduinoConnection;
        _eventBus = eventBus;
        _arduinoConnection.MessageReceived += OnMessageReceived;
    }

    private void OnMessageReceived(string message)
    {
        _eventBus.Publish(new ArduinoMessageReceivedEvent(message));
    }

    public void Dispose()
    {
        _arduinoConnection.MessageReceived -= OnMessageReceived;
    }
}