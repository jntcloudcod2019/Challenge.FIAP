using Microsoft.EntityFrameworkCore;
using System.Challenge.FIAP.Entities;

namespace System.Challenge.FIAP.Data;

public class ContextDB : DbContext
{
    public ContextDB(DbContextOptions<ContextDB> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.IdUser);
            
            entity.Property(e => e.IdUser)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Document)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(e => e.Document)
                .IsUnique();

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.StatusAccount)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(e => e.IdStudent);
            
            entity.Property(e => e.IdStudent)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(e => e.RegistrationNumber)
                .IsUnique();

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Cpf)
                .IsRequired()
                .HasMaxLength(14);

            entity.HasIndex(e => e.Cpf)
                .IsUnique();

            entity.Property(e => e.BirthDate)
                .IsRequired(false);

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .IsRequired(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.HasOne(e => e.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(e => e.IdUser)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.ToTable("Classes");
            entity.HasKey(e => e.IdClass);
            
            entity.Property(e => e.IdClass)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.ClassCode)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(e => e.ClassCode)
                .IsUnique();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(e => e.Capacity)
                .IsRequired()
                .HasDefaultValue(50);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.Property(e => e.Room)
                .HasMaxLength(50)
                .IsRequired(false);

            entity.Property(e => e.Status)
            .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Open");

        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");
            entity.HasKey(e => e.IdEnrollment);
            
            entity.Property(e => e.IdEnrollment)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.EnrollmentDate)
                .IsRequired()
                .HasDefaultValueSql("CAST(GETUTCDATE() AS DATE)");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.Property(e => e.Status)
            .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.IdStudent)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Class)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.IdClass)
                .OnDelete(DeleteBehavior.SetNull);
        });

    }
}