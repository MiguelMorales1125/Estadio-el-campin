using System;

namespace StadiumSystem.Domain.Entities;

public class MatchSession
{
    public int Id { get; set; }
    public string TeamLocal { get; set; } = string.Empty;
    public string TeamAway { get; set; } = string.Empty;
    public int ScoreLocal { get; set; }
    public int ScoreAway { get; set; }
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
}
