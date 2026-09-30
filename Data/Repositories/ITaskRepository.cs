using TaskManagementApp.Models;

namespace TaskManagementApp.Data.Repositories;

/// <summary>
/// 1. I create the repository interface: the ViewModels only know this contract
/// In Program.cs I register EfTaskRepository (PostgreSQL) or InMemoryTaskRepository (dictionaries)
/// depending on DataSettings.UseDatabase, and the rest of the app doesn't change
/// </summary>
public interface ITaskRepository
{
    // 2. System categories + the user's own categories
    Task<List<Category>> GetCategoriesAsync(string? userId);

    // 3. The user's tasks with filters applied
    Task<List<TaskItem>> GetTasksAsync(string? userId, TaskFilter filter);

    // 4. The latest tasks created by the user
    Task<List<TaskItem>> GetRecentTasksAsync(string? userId, int count);

    // 5. Numbers for the dashboard
    Task<TaskStats> GetStatsAsync(string? userId);

    // 6. Create a task (the repository assigns the Id)
    Task AddAsync(TaskItem task);

    // 7. Update a task: returns false if it doesn't exist or belongs to another user
    Task<bool> UpdateAsync(TaskItem task);

    // 8. Delete a task: returns false if it doesn't exist or belongs to another user
    Task<bool> DeleteAsync(int taskId, string? userId);
}
