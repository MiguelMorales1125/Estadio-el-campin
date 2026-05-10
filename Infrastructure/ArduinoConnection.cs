using StadiumSystem.Commands;
using StadiumSystem.Services;

namespace StadiumSystem.Infrastructure;

public class ArduinoConnection : IDisposable
{
    private readonly ISerialPortAdapter _port;
    private readonly string _portName = string.Empty;
    private static ArduinoConnection? _instance;
    private readonly StadiumSystem.Services.ITerminalLogService? _logService;

    private readonly object _bufferLock = new();
    private string _buffer = string.Empty;

    public event Action<string>? MessageReceived;

    public ArduinoConnection(StadiumSystem.Services.ITerminalLogService? logService = null, ISerialPortAdapter? portAdapter = null)
    {
        _logService = logService;
        if (portAdapter != null)
        {
            _port = portAdapter;
        }
        else
        {
            _portName = System.Environment.GetEnvironmentVariable("COM_PORT") ?? "COM3";
            int baud = int.TryParse(
                System.Environment.GetEnvironmentVariable("COM_BAUD"),
                out var parsedBaud) ? parsedBaud : 9600;
            _port = new RealSerialPortAdapter(_portName, baud, _logService);
        }
    }

    public static ArduinoConnection GetInstance(ISerialPortAdapter? adapter = null)
    {
        _instance ??= new ArduinoConnection(portAdapter: adapter);
        return _instance;
    }

    public void SendCommand(ICommand command)
    {
        SendRawCommand(command.Name);
    }

    public void SendRawCommand(string message)
    {
        if (!_port.IsOpen) return;
        try { _port.WriteLine(message); }
        catch { }
    }

    public void RequestInventory()
    {
        if (!_port.IsOpen) return;
        try { _port.WriteLine("REQUEST_INVENTORY"); }
        catch { }
    }

    public bool StartListening()
    {
        if (_port.IsOpen)
        {
            _logService?.Add($"Puerto ya está abierto");
            return true;
        }
        try
        {
            _logService?.Add(LogLevel.Info, $"Intentando abrir puerto: {_portName}");
            bool opened = _port.Open();
            if (!opened)
            {
                _logService?.Add(LogLevel.Warn, $"No se pudo abrir puerto: {_portName}");
                return false;
            }
            _logService?.Add(LogLevel.Info, $"Puerto abierto exitosamente: {_portName}");
            _port.DataReceived += OnDataReceived;
            _logService?.Add(LogLevel.Debug, $"Suscripción completada, escuchando datos");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.Add($"Error al abrir {_portName}: {ex.Message}");
            return false;
        }
    }

    private void OnDataReceived(object? sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            _logService?.Add(LogLevel.Debug, "OnDataReceived evento disparado");
            string chunk = _port.ReadExisting();
            _logService?.Add(LogLevel.Debug, $"Datos leídos: '{chunk}' (length={chunk.Length})");
            lock (_bufferLock)
            {
                _buffer += chunk;
                int idx;
                while ((idx = _buffer.IndexOf('\n')) >= 0)
                {
                    string line = _buffer.Substring(0, idx).Trim('\r', '\n', ' ', '\t');
                    _buffer = _buffer.Substring(idx + 1);
                    _logService?.Add(LogLevel.Info, $"Línea completada: '{line}'");
                    if (!string.IsNullOrEmpty(line)) ProcessIncomingMessage(line);
                }
            }
        }
        catch (Exception ex)
        {
            _logService?.Add(LogLevel.Error, $"Error en OnDataReceived: {ex.GetType().Name} - {ex.Message}");
        }
    }

    public void ProcessIncomingMessage(string message)
    {
        MessageReceived?.Invoke(message);
    }

    public void Dispose()
    {
        try
        {
            _port?.Dispose();
        }
        catch { }
    }
}
