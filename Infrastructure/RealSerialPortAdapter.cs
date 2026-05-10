using System.IO;
using System.Collections.Concurrent;
using StadiumSystem.Services;

namespace StadiumSystem.Infrastructure;

public class RealSerialPortAdapter : ISerialPortAdapter
{
    private readonly string _portName;
    private FileStream? _fileStream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool _isOpen = false;
    private CancellationTokenSource _cts = new();
    private readonly ITerminalLogService? _logService;

    // Buffer thread-safe para datos recibidos
    private readonly ConcurrentQueue<char> _dataBuffer = new();
    private readonly object _bufferLock = new();

    public bool IsOpen => _isOpen;

    public event EventHandler<SerialDataReceivedEventArgs>? DataReceived;

    public RealSerialPortAdapter(string portName, int baudRate, ITerminalLogService? logService = null)
    {
        _portName = portName;
        _logService = logService;
    }

    public bool Open()
    {
        try
        {
            if (_isOpen) return true;

            // Abre el archivo del dispositivo/PTY
            _fileStream = new FileStream(
                _portName,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None,
                4096,
                useAsync: true
            );

            _reader = new StreamReader(_fileStream, bufferSize: 4096);
            _writer = new StreamWriter(_fileStream, bufferSize: 4096) { AutoFlush = true };

            _isOpen = true;
            _cts = new CancellationTokenSource();

            // Inicia lectura asincrónica en background
            _ = ReadLoopAsync(_cts.Token);

            _logService?.Add($"Puerto abierto: {_portName}");
            return true;
        }
        catch (Exception ex)
        {
            _logService?.Add($"Error al abrir {_portName}: {ex.GetType().Name} - {ex.Message}");
            _isOpen = false;
            Cleanup();
            return false;
        }
    }

    private async Task ReadLoopAsync(CancellationToken ct)
    {
        try
        {
            char[] buffer = new char[1024];
            
            while (_isOpen && !ct.IsCancellationRequested && _reader != null)
            {
                int charsRead = await _reader.ReadAsync(buffer, 0, buffer.Length);
                
                if (charsRead > 0)
                {
                    _logService?.Add(LogLevel.Debug, $"Leyendo {charsRead} caracteres");
                    // Añadir caracteres al buffer
                    for (int i = 0; i < charsRead; i++)
                    {
                        _dataBuffer.Enqueue(buffer[i]);
                    }

                    // Disparar evento de datos recibidos
                    DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(1));
                }
                else
                {
                    // EOF - puerto cerrado por el otro lado
                    _logService?.Add(LogLevel.Debug, "EOF detected, saliendo del ReadLoop");
                    break;
                }
            }
        }
        catch (ObjectDisposedException)
        {
            // Normal cuando se cierra
        }
        catch (OperationCanceledException)
        {
            // Normal cuando se cancela
        }
        catch (Exception ex)
        {
            if (_isOpen)
                {
                _logService?.Add(LogLevel.Error, $"Error en ReadLoop: {ex.GetType().Name} - {ex.Message}");
                _logService?.Add(LogLevel.Debug, $"Stack trace: {ex.StackTrace}");
            }
        }
    }

    public void Close()
    {
        if (!_isOpen) return;

        try
        {
            _isOpen = false;
            _cts.Cancel();
            Thread.Sleep(100);
            Cleanup();
            _logService?.Add($"Puerto cerrado: {_portName}");
        }
        catch (Exception ex)
        {
            _logService?.Add($"Error al cerrar: {ex.Message}");
        }
    }

    private void Cleanup()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _fileStream?.Dispose();
        _cts?.Dispose();
    }

    public void WriteLine(string data)
    {
        if (!_isOpen || _writer == null) return;

        try
        {
            _writer.WriteLine(data);
        }
        catch (Exception ex)
        {
            _logService?.Add($"Error al escribir: {ex.Message}");
        }
    }

    public string ReadExisting()
    {
        if (!_isOpen) return string.Empty;

        try
        {
            lock (_bufferLock)
            {
                var sb = new System.Text.StringBuilder();
                while (_dataBuffer.TryDequeue(out char c))
                {
                    sb.Append(c);
                }
                return sb.ToString();
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    public void Dispose()
    {
        Close();
    }
}

