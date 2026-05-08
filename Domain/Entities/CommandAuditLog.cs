using System;
using StadiumSystem.Domain.Enums;

namespace StadiumSystem.Domain.Entities;

public class CommandAuditLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string CommandType { get; set; } = string.Empty;
    public string CommandCategory { get; set; } = "VIEW";
    public string? AdditionalData { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
