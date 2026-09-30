using TaskManagementApp.Data.Repositories;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels;

/// <summary>
/// 1. I create the ViewModel of the Tasks page
/// Here I keep the CRUD logic (Create, Read, Update, Delete) and the filters
/// </summary>
public class TasksViewModel : BaseViewModel
{
    // 2. Repository with the data (PostgreSQL or in-memory, decided in Program.cs)
    private readonly ITaskRepository _repository;

    // 3. Load counter: if the user types fast I keep only the result of the latest search
    private int _loadVersion;

    // 4. Tasks shown in the table
    private List<TaskItem> _tasks = new();
    public List<TaskItem> Tasks
    {
        get => _tasks;
        set => SetProperty(ref _tasks, value);
    }

    // 5. Task being created or edited in the form (null = form closed)
    private TaskItem? _selectedTask;
    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set => SetProperty(ref _selectedTask, value);
    }

    // 6. If the selected task already has an Id, I'm editing it
    public bool IsEditing => SelectedTask is not null && SelectedTask.Id != 0;

    // 7. Status filter (null = all)
    private Models.TaskStatus? _filterStatus;
    public Models.TaskStatus? FilterStatus
    {
        get => _filterStatus;
        set => SetProperty(ref _filterStatus, value);
    }

    // 8. Priority filter (null = all)
    private PriorityLevel? _filterPriority;
    public PriorityLevel? FilterPriority
    {
        get => _filterPriority;
        set => SetProperty(ref _filterPriority, value);
    }

    // 9. Search text on title and description
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    // 10. Categories for the form select
    private List<Category> _categories = new();
    public List<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    // 11. With this constructor I receive the repository from DI
    public TasksViewModel(ITaskRepository repository)
    {
        _repository = repository;
        Title = "Tasks";
    }

    // 12. I load categories and tasks with the current filters
    public override async Task LoadAsync()
    {
        var version = ++_loadVersion;
        try
        {
            IsLoading = Tasks.Count == 0;

            var categories = await _repository.GetCategoriesAsync(CurrentUserId);
            var tasks = await _repository.GetTasksAsync(
                CurrentUserId, new TaskFilter(FilterStatus, FilterPriority, SearchText));

            // 13. If another load started in the meantime, this result is old and I drop it
            if (version != _loadVersion)
            {
                return;
            }

            Categories = categories;
            Tasks = tasks;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading the tasks: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // 14. I save the task of the form: Id 0 = new, otherwise update
    public async Task SaveTaskAsync()
    {
        if (SelectedTask is null)
        {
            return;
        }

        try
        {
            ErrorMessage = string.Empty;
            var task = SelectedTask;

            // 15. PostgreSQL wants UTC dates: the date from the form has no Kind, so I set it
            if (task.DueDate.HasValue)
            {
                task.DueDate = DateTime.SpecifyKind(task.DueDate.Value.Date, DateTimeKind.Utc);
            }

            task.UserId = CurrentUserId;

            if (task.Id == 0)
            {
                task.CreatedAt = DateTime.UtcNow;
                task.UpdatedAt = DateTime.UtcNow;
                await _repository.AddAsync(task);
                SuccessMessage = "Task created.";
            }
            else
            {
                if (!await _repository.UpdateAsync(task))
                {
                    ErrorMessage = "Task not found.";
                    return;
                }
                SuccessMessage = "Task updated.";
            }

            // 16. I close the form, reload the list and hide the message after a few seconds
            SelectedTask = null;
            ClearMessagesAfterDelay();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving the task: {ex.Message}";
        }
    }

    // 17. I delete a task
    public async Task DeleteTaskAsync(int taskId)
    {
        try
        {
            if (!await _repository.DeleteAsync(taskId, CurrentUserId))
            {
                ErrorMessage = "Task not found.";
                return;
            }

            SuccessMessage = "Task deleted.";
            ClearMessagesAfterDelay();
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting the task: {ex.Message}";
        }
    }

    // 18. I open an empty form (first category preselected)
    public void OpenCreateForm()
    {
        SelectedTask = new TaskItem
        {
            Title = string.Empty,
            CategoryId = Categories.FirstOrDefault()?.Id ?? 0
        };
    }

    // 19. I open the edit form on a COPY, so Cancel doesn't change the row in the table
    public void OpenEditForm(TaskItem task)
    {
        SelectedTask = new TaskItem
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CategoryId = task.CategoryId,
            CreatedAt = task.CreatedAt
        };
    }

    // 20. I close the form without saving
    public void CloseForm() => SelectedTask = null;
}
