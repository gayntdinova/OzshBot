using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<AccessRight> AccessRights { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.TgId).IsUnique();
            entity.HasIndex(u => u.TgName).IsUnique();
            entity.Property(u => u.TgName).IsRequired();
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Year).IsRequired();

            entity.Property(s => s.Season).IsRequired();

            entity.HasIndex(s => new { s.Year, s.Season }).IsUnique();
        });

        modelBuilder.Entity<AccessRight>(entity =>
        {
            entity.HasKey(ar => ar.UserId);
            entity.HasOne(ar => ar.User)
                  .WithOne(u => u.AccessRight)
                  .HasForeignKey<AccessRight>(ar => ar.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(ar => ar.Rights).IsRequired();
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(p => p.PersonId);
            entity.HasIndex(p => p.UserId).IsUnique();
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Person)
                  .HasForeignKey<Person>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Surname).IsRequired();

            entity.HasIndex(p => p.Email).IsUnique();
            entity.HasIndex(p => p.Phone).IsUnique();
        });
    }
}