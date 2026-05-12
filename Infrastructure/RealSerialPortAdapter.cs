using System.IO.Ports;
using StadiumSystem.Services;

namespace StadiumSystem.Infrastructure;

public class RealSerialPortAdapter : ISerialPortAdapter
{
    private SerialPort? _port;
    private readonly string _portName;
    private readonly int _baudRate;
    private readonly ITerminalLogService? _logService;

    public bool IsOpen => _port?.IsOpen ?? false;

    public event SerialDataReceivedEventHandler? DataReceived;

    public RealSerialPortAdapter(string portName, int baudRate, ITerminalLogService? logService = null)
    {
        _portName = portName;
        _baudRate = baudRate;
        _logService = logService;
    }

    public bool Open()
    {
        try
        {
            _port = new SerialPort(_portName, _baudRate)
            {
                ReadTimeout = 1000,
                WriteTimeout = 2000
            };
            _port.DataReceived += OnSerialDataReceived;
            _port.Open();
            _logService?.Add($"Puerto abierto: {_portName} @ {_baudRate}");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.Add($"Error al abrir {_portName}: {ex.GetType().Name} - {ex.Message}");
            return false;
        }
    }

    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        DataReceived?.Invoke(this, e);
    }

    public void Close()
    {
        try
        {
            if (_port is { IsOpen: true })
            {
                _port.DataReceived -= OnSerialDataReceived;
                _port.Close();
                _port.Dispose();
                _port = null;
                _logService?.Add($"Puerto cerrado: {_portName}");
            }
        }
        catch { }
    }

    public void WriteLine(string data)
    {
        try
        {
            _port?.WriteLine(data);
        }
        catch (Exception ex)
        {
            _logService?.Add($"Error al escribir: {ex.Message}");
        }
    }

    public string ReadExisting()
    {
        try
        {
            return _port?.ReadExisting() ?? "";
        }
        catch
        {
            return "";
        }
    }

    public void Dispose() => Close();
}
