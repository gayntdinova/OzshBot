using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Student> People { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<ChildParent> PeopleParents;
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
            });

            modelBuilder.Entity<Counsellor>(entity =>
            {
                entity.HasKey(p => p.CounsellorId);
                entity.HasIndex(p => p.UserId).IsUnique();
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Counsellor)
                      .HasForeignKey<Student>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(p => p.Name).IsRequired();
                entity.Property(p => p.Surname).IsRequired();

                entity.HasIndex(p => p.Email).IsUnique();
                entity.HasIndex(p => p.Phone).IsUnique();
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
                entity.HasKey(pp => new { pp.ChildId, pp.ParentId});
                entity.Property(pp => pp.ChildId).IsRequired();
                entity.HasIndex(pp => pp.ChildId);
                entity.Property(pp => pp.ParentId).IsRequired();
                entity.Property(pp => pp.ParentId);
                entity.HasOne(pp => pp.Child)
                      .WithMany(p => p.Relations)
                      .HasForeignKey(pp => pp.ChildId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(pp => pp.Parent)
                      .WithMany(p => p.Relations)
                      .HasForeignKey(pp => pp.ParentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}