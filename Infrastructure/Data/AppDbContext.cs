using Microsoft.EntityFrameworkCore;
using StadiumSystem.Domain.Entities;

namespace StadiumSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<CommandAuditLog> AuditLogs { get; set; }
    public DbSet<StadiumState> StadiumStates { get; set; }
    public DbSet<MatchSession> Matches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Se cargan las variables desde el archivo .env
        DotNetEnv.Env.Load();
        string host = System.Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
        string dbName = System.Environment.GetEnvironmentVariable("DB_NAME") ?? "estadio_db";
        string user = System.Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
        string pass = System.Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
        
        optionsBuilder.UseNpgsql($"Host={host};Database={dbName};Username={user};Password={pass}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
