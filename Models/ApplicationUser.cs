using Microsoft.AspNetCore.Identity;

namespace TaskManagementApp.Models;

/// <summary>
/// 1. I extend IdentityUser to create the application user
/// I inherit everything from Identity (Id, Email, PasswordHash, ...) and add my own properties
/// </summary>
public class ApplicationUser : IdentityUser
{
    // 2. Full name
    public string? FullName { get; set; }

    // 3. Sign-up date
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // 4. Last login
    public DateTime? LastLoginAt { get; set; }

    // 5. Setting: how many tasks per page (default 10)
    public int TasksPerPage { get; set; } = 10;

    // 6. One user has many tasks
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    // 7. One user has many categories
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
