using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using TaskManagementApp.ViewModels;

namespace TaskManagementApp.Components;

/// <summary>
/// 1. I create a base class for every page linked to a ViewModel
/// The repetitive code (injection, UI refresh, logged-in user) lives here only once
/// In the pages I use it with @inherits ViewModelPageBase&lt;MyViewModel&gt;
/// </summary>
public abstract class ViewModelPageBase<TViewModel> : ComponentBase, IDisposable
    where TViewModel : BaseViewModel
{
    // 2. I inject the page ViewModel: in the .razor I use it as VM
    [Inject]
    protected TViewModel VM { get; set; } = default!;

    // 3. Authentication state, to know who is logged in
    [CascadingParameter]
    protected Task<AuthenticationState>? AuthState { get; set; }

    // 4. Logged-in user (for example to show the name in the page)
    protected ClaimsPrincipal? CurrentUser { get; private set; }

    // 5. On load I subscribe to the ViewModel, pass the user id and load the data
    protected override async Task OnInitializedAsync()
    {
        VM.PropertyChanged += OnViewModelChanged;

        if (AuthState is not null)
        {
            CurrentUser = (await AuthState).User;
            VM.CurrentUserId = CurrentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        await VM.LoadAsync();
    }

    // 6. Every time the ViewModel changes a property I re-render the page
    // (InvokeAsync because the notification can come from another thread, e.g. ClearMessagesAfterDelay)
    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
        => _ = InvokeAsync(StateHasChanged);

    // 7. When the page is closed I unsubscribe from the ViewModel
    public void Dispose()
    {
        VM.PropertyChanged -= OnViewModelChanged;
        GC.SuppressFinalize(this);
    }
}
