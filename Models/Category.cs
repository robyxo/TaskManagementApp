namespace TaskManagementApp.Models;

/// <summary>
/// 1. I create the Category model - I use it to group tasks (Work, Personal, Study, ...)
/// Categories with UserId = null are system categories shared by everyone
/// </summary>
public class Category
{
    // 2. Unique identifier
    public int Id { get; set; }

    // 3. Name - unique for each user
    public required string Name { get; set; }

    // 4. Optional description
    public string? Description { get; set; }

    // 5. Badge color in the UI
    public string Color { get; set; } = "#0d6efd";

    // 6. Owner (null = system category)
    public string? UserId { get; set; }

    // 7. One category has many tasks
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();

    // 8. Creation date
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 9. Last update date
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
