using System.IO.Ports;
using StadiumSystem.Commands;

namespace StadiumSystem.Infrastructure;


public class ArduinoConnection : IDisposable
{
    private readonly SerialPort _port;
    private static ArduinoConnection? _instance;

    private readonly object _bufferLock = new();
    private string _buffer = string.Empty;

    public event Action<string>? MessageReceived;

    private ArduinoConnection()
    {
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

    public static ArduinoConnection GetInstance()
    {
        _instance ??= new ArduinoConnection();
        return _instance;
    }

    public void SendCommand(ICommand command)
    {
        if (!_port.IsOpen) return;
        try { _port.WriteLine(command.Name); }
        catch { }
    }

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

    public void ProcessIncomingMessage(string message)
    {
        MessageReceived?.Invoke(message);
    }

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
