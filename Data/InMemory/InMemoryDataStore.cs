using System.Collections.Concurrent;
using TaskManagementApp.Models;

namespace TaskManagementApp.Data.InMemory;

/// <summary>
/// 1. I create the in-memory "database": two dictionaries keyed by Id
/// It is registered as a Singleton, so all users share the same data while the app is running
/// I fill it at startup from Data/Seed/task-data.json (see DemoDataSeeder)
/// </summary>
public class InMemoryDataStore
{
    // 2. ConcurrentDictionary because in Blazor Server many users can write at the same time
    public ConcurrentDictionary<int, Category> Categories { get; } = new();
    public ConcurrentDictionary<int, TaskItem> Tasks { get; } = new();

    // 3. Counters that play the role of the database IDENTITY columns
    private int _lastCategoryId;
    private int _lastTaskId;

    // 4. Interlocked makes the increment thread-safe
    public int NextCategoryId() => Interlocked.Increment(ref _lastCategoryId);
    public int NextTaskId() => Interlocked.Increment(ref _lastTaskId);
}
