namespace StadiumSystem.Domain.Events;

public class MovementDetectedEventArgs : EventArgs
{
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}
