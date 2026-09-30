using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskManagementApp.ViewModels;

/// <summary>
/// 1. I create a base class that extends ObservableObject from the MVVM Toolkit
/// It holds the properties that ALL the ViewModels of the app need
/// </summary>
public class BaseViewModel : ObservableObject
{
    // 2. Page title
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    // 3. Loading flag - I show a spinner while it's true
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    // 4. Error message shown to the user
    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    // 5. Success message shown after an operation
    private string _successMessage = string.Empty;
    public string SuccessMessage
    {
        get => _successMessage;
        set => SetProperty(ref _successMessage, value);
    }

    // 6. Id of the logged-in user: the page sets it (ViewModelPageBase) so I can filter the data
    public string? CurrentUserId { get; set; }

    // 7. Virtual method every ViewModel overrides to load its data
    public virtual Task LoadAsync() => Task.CompletedTask;

    // 8. Helper that clears the messages after a few seconds
    protected void ClearMessagesAfterDelay(int delayMs = 3000)
    {
        Task.Delay(delayMs).ContinueWith(_ =>
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        });
    }
}
