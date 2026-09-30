using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TaskManagementApp.Models;
using TaskManagementApp.Utils;

namespace TaskManagementApp.Components.Pages;

/// <summary>
/// 1. I create the code-behind of the Tasks page
/// The .razor stays UI only: here I keep route, services and event handlers
/// </summary>
[Route(AppRoutes.Tasks)]
[Authorize]
public partial class Tasks
{
    // 2. I inject IJSRuntime to show the browser confirm dialog before deleting
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    // 3. All the enum values, to fill the selects without writing the options by hand
    private static readonly Models.TaskStatus[] AllStatuses = Enum.GetValues<Models.TaskStatus>();
    private static readonly PriorityLevel[] AllPriorities = Enum.GetValues<PriorityLevel>();

    // 4. Shortcut for the form: the form is shown only when SelectedTask is not null
    private TaskItem EditingTask => VM.SelectedTask!;

    // 5. I convert the select value to the enum (empty string = no filter)
    private async Task OnStatusFilterChanged(ChangeEventArgs e)
    {
        VM.FilterStatus = Enum.TryParse<Models.TaskStatus>(e.Value?.ToString(), out var status)
            ? status
            : null;
        await VM.LoadAsync();
    }

    // 6. Same thing for the priority
    private async Task OnPriorityFilterChanged(ChangeEventArgs e)
    {
        VM.FilterPriority = Enum.TryParse<PriorityLevel>(e.Value?.ToString(), out var priority)
            ? priority
            : null;
        await VM.LoadAsync();
    }

    // 7. I ask for confirmation with JavaScript confirm() and delete if the user accepts
    private async Task ConfirmDeleteAsync(TaskItem task)
    {
        var confirmed = await JS.InvokeAsync<bool>("confirm", $"Delete \"{task.Title}\"?");
        if (confirmed)
        {
            await VM.DeleteTaskAsync(task.Id);
        }
    }

    // 8. I truncate the description to 60 characters for the table
    private static string ShortDescription(string text)
        => text.Length <= 60 ? text : text[..60] + "…";
}
