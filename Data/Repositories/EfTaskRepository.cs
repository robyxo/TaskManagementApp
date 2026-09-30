using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Data.Repositories;

/// <summary>
/// 1. I create the PostgreSQL implementation of the repository with Entity Framework Core
/// I use IDbContextFactory: every method opens its own short-lived DbContext,
/// which is the recommended pattern in Blazor Server (a DbContext can't run two queries at once)
/// </summary>
public class EfTaskRepository : ITaskRepository
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    // 2. With this constructor I receive the factory from DI so I can call it in every method
    public EfTaskRepository(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // 3. System categories (UserId null) + the user's categories
    public async Task<List<Category>> GetCategoriesAsync(string? userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.Categories
            .AsNoTracking()
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // 4. Tasks with filters
    public async Task<List<TaskItem>> GetTasksAsync(string? userId, TaskFilter filter)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        IQueryable<TaskItem> query = db.Tasks
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        // 5. Case-insensitive search with PostgreSQL ILIKE
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var pattern = $"%{filter.SearchText.Trim()}%";
            query = query.Where(t => EF.Functions.ILike(t.Title, pattern) ||
                                     (t.Description != null && EF.Functions.ILike(t.Description, pattern)));
        }

        // 6. First the tasks with a due date (closest first), then the others (newest first)
        return await query
            .OrderBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    // 7. Latest tasks
    public async Task<List<TaskItem>> GetRecentTasksAsync(string? userId, int count)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        return await db.Tasks
            .AsNoTracking()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    // 8. Dashboard numbers: I reuse the same base query for every count
    public async Task<TaskStats> GetStatsAsync(string? userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var userTasks = db.Tasks.Where(t => t.UserId == userId);

        return new TaskStats(
            await userTasks.CountAsync(),
            await userTasks.CountAsync(t => t.Status == Models.TaskStatus.Completed),
            await userTasks.CountAsync(t => t.Status == Models.TaskStatus.InProgress),
            await db.Categories.CountAsync(c => c.UserId == null || c.UserId == userId));
    }

    // 9. New task
    public async Task AddAsync(TaskItem task)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // 10. I only set the foreign key, so EF doesn't try to insert the category again
        task.Category = null;
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
    }

    // 11. Update: I load the tracked entity and copy the edited fields
    public async Task<bool> UpdateAsync(TaskItem task)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var existing = await db.Tasks.FirstOrDefaultAsync(t => t.Id == task.Id && t.UserId == task.UserId);
        if (existing is null)
        {
            return false;
        }

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Status = task.Status;
        existing.Priority = task.Priority;
        existing.DueDate = task.DueDate;
        existing.CategoryId = task.CategoryId;
        existing.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return true;
    }

    // 12. Delete
    public async Task<bool> DeleteAsync(int taskId, string? userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
        if (task is null)
        {
            return false;
        }

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return true;
    }
}
