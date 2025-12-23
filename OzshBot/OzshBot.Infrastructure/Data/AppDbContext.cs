using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Counsellor> Counsellors { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<ChildParent> ChildrenParents { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<StudentSession> StudentsSessions { get; set; }
    public DbSet<CounsellorSession> CounsellorsSessions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.HasIndex(u => u.TgId).IsUnique();
            entity.HasIndex(u => u.TgName).IsUnique();
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.SessionId);
            entity.Property(s => s.StartDate).IsRequired();
            entity.Property(s => s.EndDate).IsRequired();

            entity.HasMany(s => s.StudentsRelations)
                  .WithOne(ss => ss.Session)
                  .HasForeignKey(ss => ss.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(s => s.CounsellorsRelations)
                  .WithOne(cs => cs.Session)
                  .HasForeignKey(cs => cs.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(p => p.StudentId);
            entity.HasIndex(p => p.UserId).IsUnique();
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Student)
                  .HasForeignKey<Student>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Surname).IsRequired();

            entity.Property(p => p.BirthDate).IsRequired();

            entity.Property(p => p.Email).IsRequired();
            entity.HasIndex(p => p.Email).IsUnique();

            entity.Property(p => p.Phone).IsRequired();
            entity.HasIndex(p => p.Phone).IsUnique();

            entity.HasMany(s => s.SessionRelations)
                  .WithOne(ss => ss.Student)
                  .HasForeignKey(ss => ss.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Counsellor>(entity =>
        {
            entity.HasKey(p => p.CounsellorId);
            entity.HasIndex(p => p.UserId).IsUnique();
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Counsellor)
                  .HasForeignKey<Counsellor>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Surname).IsRequired();

            entity.Property(p => p.Email).IsRequired();
            entity.HasIndex(p => p.Email).IsUnique();

            entity.Property(p => p.Phone).IsRequired();
            entity.HasIndex(p => p.Phone).IsUnique();

            entity.HasMany(c => c.SessionRelations)
                  .WithOne(cs => cs.Counsellor)
                  .HasForeignKey(cs => cs.CounsellorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(p => p.ParentId);
            entity.HasIndex(p => p.Phone).IsUnique();
            entity.Property(p => p.Name).IsRequired();
            entity.Property(p => p.Surname).IsRequired();
        });

        modelBuilder.Entity<ChildParent>(entity =>
        {
            entity.HasKey(pp => new { pp.ChildId, pp.ParentId });
            entity.Property(pp => pp.ChildId).IsRequired();
            entity.HasIndex(pp => pp.ChildId);
            entity.Property(pp => pp.ParentId).IsRequired();
            entity.Property(pp => pp.ParentId);
            entity.HasOne(pp => pp.Child)
                  .WithMany(p => p.ParentRelations)
                  .HasForeignKey(pp => pp.ChildId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pp => pp.Parent)
                  .WithMany(p => p.Relations)
                  .HasForeignKey(pp => pp.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudentSession>(entity =>
        {
            entity.HasKey(ss => new { ss.StudentId, ss.SessionId });

            entity.HasOne(ss => ss.Student)
                  .WithMany(s => s.SessionRelations)
                  .HasForeignKey(ss => ss.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ss => ss.Session)
                  .WithMany(s => s.StudentsRelations)
                  .HasForeignKey(ss => ss.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ss => ss.StudentId);
            entity.HasIndex(ss => ss.SessionId);
        });
        
        modelBuilder.Entity<CounsellorSession>(entity =>
        {
            entity.HasKey(cs => new { cs.CounsellorId, cs.SessionId });
            
            entity.HasOne(cs => cs.Counsellor)
                  .WithMany(c => c.SessionRelations)
                  .HasForeignKey(cs => cs.CounsellorId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(cs => cs.Session)
                  .WithMany(s => s.CounsellorsRelations)
                  .HasForeignKey(cs => cs.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(cs => cs.CounsellorId);
            entity.HasIndex(cs => cs.SessionId);
        });
    }
}
