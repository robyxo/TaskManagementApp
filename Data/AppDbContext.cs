using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Data;

/// <summary>
/// 1. I create the DbContext that inherits from IdentityDbContext
/// IdentityDbContext already gives me all the Identity tables (Users, Roles, Claims, ...)
/// I add my own tables: Tasks and Categories
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    // 2. Constructor that receives the options from the DI container
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // 3. Tasks table
    public DbSet<TaskItem> Tasks { get; set; }

    // 4. Categories table
    public DbSet<Category> Categories { get; set; }

    // 5. Relationships and constraints
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 6. I call the base method so Identity configures its own tables
        base.OnModelCreating(modelBuilder);

        // 7. Task -> Category (many to one): I can't delete a category that still has tasks
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // 8. Task -> ApplicationUser (many to one): if I delete a user, I delete their tasks
        modelBuilder.Entity<TaskItem>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 9. Category -> ApplicationUser (many to one)
        modelBuilder.Entity<Category>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 10. Indexes to speed up searches and per-user filtering
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.Title);
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.UserId);

        // 11. A category name must be unique for each user
        modelBuilder.Entity<Category>()
            .HasIndex(c => new { c.UserId, c.Name })
            .IsUnique();
    }
}
