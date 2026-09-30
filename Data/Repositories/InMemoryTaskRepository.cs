using TaskManagementApp.Data.InMemory;
using TaskManagementApp.Models;

namespace TaskManagementApp.Data.Repositories;

/// <summary>
/// 1. I create the in-memory implementation of the repository: same contract as EfTaskRepository,
/// but the data lives in the dictionaries of InMemoryDataStore (LINQ to Objects instead of SQL)
/// I always return COPIES of the objects, so the UI never edits the stored data by mistake
/// (this is the same behaviour I get from EF with AsNoTracking)
/// </summary>
public class InMemoryTaskRepository : ITaskRepository
{
    private readonly InMemoryDataStore _store;

    // 2. With this constructor I receive the singleton store from DI
    public InMemoryTaskRepository(InMemoryDataStore store)
    {
        _store = store;
    }

    // 3. System categories + the user's categories
    public Task<List<Category>> GetCategoriesAsync(string? userId)
    {
        var categories = _store.Categories.Values
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToList();

        return Task.FromResult(categories);
    }

    // 4. Tasks with filters
    public Task<List<TaskItem>> GetTasksAsync(string? userId, TaskFilter filter)
    {
        var query = _store.Tasks.Values.Where(t => t.UserId == userId);

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        // 5. Case-insensitive search, like ILIKE in PostgreSQL
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var text = filter.SearchText.Trim();
            query = query.Where(t => t.Title.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                                     (t.Description != null && t.Description.Contains(text, StringComparison.OrdinalIgnoreCase)));
        }

        // 6. Same ordering as the database version
        var tasks = query
            .OrderBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedAt)
            .Select(Copy)
            .ToList();

        return Task.FromResult(tasks);
    }

    // 7. Latest tasks
    public Task<List<TaskItem>> GetRecentTasksAsync(string? userId, int count)
    {
        var tasks = _store.Tasks.Values
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Select(Copy)
            .ToList();

        return Task.FromResult(tasks);
    }

    // 8. Dashboard numbers
    public Task<TaskStats> GetStatsAsync(string? userId)
    {
        var userTasks = _store.Tasks.Values.Where(t => t.UserId == userId).ToList();

        var stats = new TaskStats(
            userTasks.Count,
            userTasks.Count(t => t.Status == Models.TaskStatus.Completed),
            userTasks.Count(t => t.Status == Models.TaskStatus.InProgress),
            _store.Categories.Values.Count(c => c.UserId == null || c.UserId == userId));

        return Task.FromResult(stats);
    }

    // 9. New task: I assign the Id myself, like the database would
    public Task AddAsync(TaskItem task)
    {
        task.Id = _store.NextTaskId();
        task.Category = null;
        _store.Tasks[task.Id] = Copy(task);
        return Task.CompletedTask;
    }

    // 10. Update: I check that the task exists and belongs to the same user
    public Task<bool> UpdateAsync(TaskItem task)
    {
        if (!_store.Tasks.TryGetValue(task.Id, out var existing) || existing.UserId != task.UserId)
        {
            return Task.FromResult(false);
        }

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Status = task.Status;
        existing.Priority = task.Priority;
        existing.DueDate = task.DueDate;
        existing.CategoryId = task.CategoryId;
        existing.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult(true);
    }

    // 11. Delete
    public Task<bool> DeleteAsync(int taskId, string? userId)
    {
        if (!_store.Tasks.TryGetValue(taskId, out var existing) || existing.UserId != userId)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(_store.Tasks.TryRemove(taskId, out _));
    }

    // 12. I create a detached copy and attach the category (like an EF Include)
    private TaskItem Copy(TaskItem source) => new()
    {
        Id = source.Id,
        Title = source.Title,
        Description = source.Description,
        Status = source.Status,
        Priority = source.Priority,
        DueDate = source.DueDate,
        CategoryId = source.CategoryId,
        Category = _store.Categories.GetValueOrDefault(source.CategoryId),
        UserId = source.UserId,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
}
