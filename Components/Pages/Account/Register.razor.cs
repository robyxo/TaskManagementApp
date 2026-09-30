using Microsoft.AspNetCore.Components;
using TaskManagementApp.Models;
using TaskManagementApp.Utils;
using TaskManagementApp.ViewModels;

namespace TaskManagementApp.Components.Pages.Account;

/// <summary>
/// 1. I create the code-behind of the Sign-up page
/// </summary>
[Route(AppRoutes.Register)]
public partial class Register
{
    // 2. ViewModel with the registration logic
    [Inject]
    private AccountViewModel VM { get; set; } = default!;

    // 3. NavigationManager to go to the dashboard after sign-up
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    // 4. Posted form data
    [SupplyParameterFromForm]
    private RegisterInput Input { get; set; } = new();

    // 5. If sign-up succeeds the user is already logged in: I send them to the dashboard
    private async Task RegisterAsync()
    {
        if (await VM.RegisterAsync(Input))
        {
            Navigation.NavigateTo(AppRoutes.Home, forceLoad: true);
        }
    }
}
