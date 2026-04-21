using System.IO.Ports;
using StadiumSystem.Commands;
using StadiumSystem.Events;

namespace StadiumSystem.Infrastructure;

/// <summary>
/// GRASP - Controller + Pure Fabrication: gestiona la comunicación serie con
/// el Arduino. Singleton garantiza una única conexión activa en el sistema.
/// </summary>
public class ArduinoConnection : IDisposable
{
    private readonly SerialPort _port;
    private readonly EventBus _eventBus;
    private static ArduinoConnection? _instance;

    private readonly object _bufferLock = new();
    private string _buffer = string.Empty;

    /// <summary>Evento disparado cuando llega una línea completa del Arduino.</summary>
    public event Action<string>? MessageReceived;

    private ArduinoConnection()
    {
        _eventBus = EventBus.GetInstance();

        string portName = System.Environment.GetEnvironmentVariable("COM_PORT") ?? "COM3";
        int baud = int.TryParse(
            System.Environment.GetEnvironmentVariable("COM_BAUD"),
            out var parsedBaud) ? parsedBaud : 9600;

        _port = new SerialPort(portName, baud)
        {
            NewLine = "\n",
            ReadTimeout = 500,
            WriteTimeout = 500,
            DtrEnable = true
        };
    }

    /// <summary>Singleton: retorna la única instancia de la conexión.</summary>
    public static ArduinoConnection GetInstance()
    {
        _instance ??= new ArduinoConnection();
        return _instance;
    }

    /// <summary>Envía un comando serializado al Arduino vía puerto serie.</summary>
    public void SendCommand(ICommand command)
    {
        if (!_port.IsOpen) return;
        try { _port.WriteLine(command.Name); }
        catch { }
    }

    /// <summary>Inicia la escucha de mensajes entrantes del Arduino.</summary>
    public bool StartListening()
    {
        if (_port.IsOpen) return true;
        try
        {
            _port.Open();
            _port.DataReceived += OnDataReceived;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Arduino] No se pudo abrir {_port.PortName}: {ex.Message}");
            return false;
        }
    }

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string chunk = _port.ReadExisting();
            lock (_bufferLock)
            {
                _buffer += chunk;
                int idx;
                while ((idx = _buffer.IndexOf('\n')) >= 0)
                {
                    string line = _buffer.Substring(0, idx).Trim('\r', '\n', ' ', '\t');
                    _buffer = _buffer.Substring(idx + 1);
                    if (!string.IsNullOrEmpty(line)) ProcessIncomingMessage(line);
                }
            }
        }
        catch { }
    }

    /// <summary>Procesa un mensaje de texto recibido y publica el IEvent correspondiente.</summary>
    public void ProcessIncomingMessage(string message)
    {
        MessageReceived?.Invoke(message);
    }

    /// <summary>Procesa el inventario de dispositivos reportado por el Arduino.</summary>
    public void ProcessInventory(string inventory) { }

    public void Dispose()
    {
        try
        {
            if (_port.IsOpen)
            {
                _port.DataReceived -= OnDataReceived;
                _port.Close();
            }
        }
        catch { }
        _port.Dispose();
    }
}
