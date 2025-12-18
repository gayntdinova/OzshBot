using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Data;

public class LogsDbContext: DbContext
{
    public DbSet<Log> Logs { get; set; }

    public LogsDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(log => log.LogId);
            entity.Property(log => log.Success).IsRequired();
        });
    }
}