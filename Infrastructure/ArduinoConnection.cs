using System.IO.Ports;
using StadiumSystem.Commands;
using StadiumSystem.Events;

namespace StadiumSystem.Infrastructure;

/// <summary>
/// GRASP - Controller + Pure Fabrication: gestiona la comunicación serie con
/// el Arduino. Singleton garantiza una única conexión activa en el sistema.
/// </summary>
public class ArduinoConnection
{
    private SerialPort _port;
    private EventBus _eventBus;
    private static ArduinoConnection? _instance;

    private ArduinoConnection()
    {
        _port     = new SerialPort();
        _eventBus = EventBus.GetInstance();
    }

    /// <summary>Singleton: retorna la única instancia de la conexión.</summary>
    public static ArduinoConnection GetInstance()
    {
        _instance ??= new ArduinoConnection();
        return _instance;
    }

    /// <summary>Envía un comando serializado al Arduino vía puerto serie.</summary>
    public void SendCommand(ICommand command) { }

    /// <summary>Inicia la escucha de mensajes entrantes del Arduino.</summary>
    public void StartListening() { }

    /// <summary>Procesa un mensaje de texto recibido y publica el IEvent correspondiente.</summary>
    public void ProcessIncomingMessage(string message) { }

    /// <summary>Procesa el inventario de dispositivos reportado por el Arduino.</summary>
    public void ProcessInventory(string inventory) { }
}
