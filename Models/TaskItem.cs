using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models;

/// <summary>
/// 1. I create the Task model - it represents one task in the database
/// The DataAnnotations are also used to validate the form in the Tasks page
/// </summary>
public class TaskItem
{
    // 2. Unique identifier
    public int Id { get; set; }

    // 3. Title - it can't be empty
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title can be at most 200 characters")]
    public required string Title { get; set; }

    // 4. Optional description
    public string? Description { get; set; }

    // 5. Status: Not Started, In Progress, Completed, Archived
    public Models.TaskStatus Status { get; set; } = Models.TaskStatus.NotStarted;

    // 6. Priority: Low, Medium, High
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    // 7. Due date
    public DateTime? DueDate { get; set; }

    // 8. Category - with [Range] I force the user to pick one in the form (0 = nothing selected)
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    // 9. Owner of the task
    public string? UserId { get; set; }

    // 10. Creation date
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 11. Last update date
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 12. Possible task statuses
/// </summary>
public enum TaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Archived = 3
}

/// <summary>
/// 13. Priority levels
/// </summary>
public enum PriorityLevel
{
    Low = 0,
    Medium = 1,
    High = 2
}
