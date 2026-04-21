using Microsoft.EntityFrameworkCore;
using StadiumSystem.Domain.Entities;
using System.IO;

namespace StadiumSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<CommandAuditLog> AuditLogs { get; set; }
    public DbSet<StadiumState> StadiumStates { get; set; }
    public DbSet<MatchSession> Matches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Carga .env desde la carpeta del ejecutable (build/publish)
        // y luego intenta el directorio de trabajo actual como fallback.
        string envFromExeDir = Path.Combine(AppContext.BaseDirectory, ".env");
        if (File.Exists(envFromExeDir))
            DotNetEnv.Env.Load(envFromExeDir);
        else
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
