using System.IO.Ports;

namespace StadiumSystem.Infrastructure;

public interface ISerialPortAdapter : IDisposable
{
    bool IsOpen { get; }
    event SerialDataReceivedEventHandler? DataReceived;
    
    bool Open();
    void Close();
    void WriteLine(string data);
    string ReadExisting();
}
