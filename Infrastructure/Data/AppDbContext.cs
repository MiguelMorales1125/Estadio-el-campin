using Microsoft.EntityFrameworkCore;
using StadiumSystem.Domain.Entities;

namespace StadiumSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<CommandAuditLog> AuditLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Se define la cadena de conexión apuntando a Docker local
        optionsBuilder.UseNpgsql("Host=localhost;Database=estadio_db;Username=postgres;Password=mi_super_password");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
