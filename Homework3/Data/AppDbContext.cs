using Homework3.Variant18.Models;
using Microsoft.EntityFrameworkCore;

namespace Homework3.Variant18.Data;

/// <summary>Контекст базы данных SQLite для приложения домашнего задания №3.</summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>Набор кафедр.</summary>
    public DbSet<Department> Departments => Set<Department>();

    /// <summary>Набор преподавателей.</summary>
    public DbSet<Teacher> Teachers => Set<Teacher>();

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=homework3_variant18.db");
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>()
            .HasMany(department => department.Teachers)
            .WithOne(teacher => teacher.Department)
            .HasForeignKey(teacher => teacher.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
