namespace TaskManagementApp.Models;

// 1. I create small records to move data between ViewModels and repositories

// 2. Filters of the Tasks page (null = no filter)
public record TaskFilter(Models.TaskStatus? Status, PriorityLevel? Priority, string? SearchText);

// 3. Numbers shown in the dashboard cards
public record TaskStats(int Total, int Completed, int InProgress, int Categories);
