using TaskManagementApp.Models;

namespace TaskManagementApp.Utils;

/// <summary>
/// 1. I create a helper that turns statuses and priorities into labels and Bootstrap colors
/// I call it from both Home and Tasks, so I don't repeat switch statements in the .razor files
/// </summary>
public static class TaskDisplay
{
    // 2. Status label
    public static string StatusLabel(Models.TaskStatus status) => status switch
    {
        Models.TaskStatus.NotStarted => "Not started",
        Models.TaskStatus.InProgress => "In progress",
        Models.TaskStatus.Completed => "Completed",
        Models.TaskStatus.Archived => "Archived",
        _ => status.ToString()
    };

    // 3. Badge CSS class for the status
    public static string StatusBadge(Models.TaskStatus status) => status switch
    {
        Models.TaskStatus.NotStarted => "bg-secondary",
        Models.TaskStatus.InProgress => "bg-warning text-dark",
        Models.TaskStatus.Completed => "bg-success",
        Models.TaskStatus.Archived => "bg-dark",
        _ => "bg-light text-dark"
    };

    // 4. Priority label
    public static string PriorityLabel(PriorityLevel priority) => priority switch
    {
        PriorityLevel.Low => "Low",
        PriorityLevel.Medium => "Medium",
        PriorityLevel.High => "High",
        _ => priority.ToString()
    };

    // 5. Badge CSS class for the priority
    public static string PriorityBadge(PriorityLevel priority) => priority switch
    {
        PriorityLevel.Low => "bg-info text-dark",
        PriorityLevel.Medium => "bg-warning text-dark",
        PriorityLevel.High => "bg-danger",
        _ => "bg-light text-dark"
    };

    // 6. Date in day/month/year format (the database stores UTC, I show local time)
    public static string Date(DateTime value) => value.ToLocalTime().ToString("dd/MM/yyyy");

    // 7. Due date: I show it as it is, because it's a calendar day, not a moment in time
    public static string DueDate(DateTime value) => value.ToString("dd/MM/yyyy");
}
