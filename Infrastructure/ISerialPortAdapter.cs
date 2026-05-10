namespace StadiumSystem.Infrastructure;

public interface ISerialPortAdapter : IDisposable
{
    bool IsOpen { get; }
    event EventHandler<SerialDataReceivedEventArgs>? DataReceived;
    
    bool Open();
    void Close();
    void WriteLine(string data);
    string ReadExisting();
}

public class SerialDataReceivedEventArgs : EventArgs
{
    public SerialDataReceivedEventArgs(int count)
    {
        EventType = SerialDataReceivedEventType.Chars;
        Count = count;
    }

    public SerialDataReceivedEventType EventType { get; }
    public int Count { get; }
}

public enum SerialDataReceivedEventType
{
    Chars = 1,
    Eof = 2
}
