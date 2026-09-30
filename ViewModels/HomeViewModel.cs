using TaskManagementApp.Data.Repositories;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels;

/// <summary>
/// 1. I create the ViewModel of the dashboard
/// I inherit from BaseViewModel to get Title, IsLoading, ErrorMessage, ...
/// </summary>
public class HomeViewModel : BaseViewModel
{
    // 2. Repository: I don't know (and don't care) if it's PostgreSQL or in-memory
    private readonly ITaskRepository _repository;

    // 3. Numbers of the dashboard cards
    private TaskStats _stats = new(0, 0, 0, 0);
    public TaskStats Stats
    {
        get => _stats;
        set => SetProperty(ref _stats, value);
    }

    // 4. Latest tasks for the table at the bottom
    private List<TaskItem> _recentTasks = new();
    public List<TaskItem> RecentTasks
    {
        get => _recentTasks;
        set => SetProperty(ref _recentTasks, value);
    }

    // 5. With this constructor I receive the repository from DI
    public HomeViewModel(ITaskRepository repository)
    {
        _repository = repository;
        Title = "Dashboard";
    }

    // 6. I load stats and latest tasks
    public override async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Stats = await _repository.GetStatsAsync(CurrentUserId);
            RecentTasks = await _repository.GetRecentTasksAsync(CurrentUserId, 5);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading the dashboard: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
