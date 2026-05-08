using StadiumSystem.Domain.Entities;
using StadiumSystem.Domain.Enums;
using StadiumSystem.Infrastructure.Data;

namespace StadiumSystem.Services;

public sealed class AuditLogService
{
    private readonly AppDbContext _dbContext;

    public AuditLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Log(User user, string commandType, CommandCategory category, string? additionalData = null)
    {
        if (user is null) return;

        _dbContext.AuditLogs.Add(new CommandAuditLog
        {
            UserId = user.Id,
            CommandType = commandType,
            CommandCategory = category.ToString(),
            AdditionalData = additionalData,
            Timestamp = DateTime.UtcNow
        });

        _dbContext.SaveChanges();
    }
}