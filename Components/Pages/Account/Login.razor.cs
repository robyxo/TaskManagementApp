using Microsoft.AspNetCore.Components;
using TaskManagementApp.Models;
using TaskManagementApp.Utils;
using TaskManagementApp.ViewModels;

namespace TaskManagementApp.Components.Pages.Account;

/// <summary>
/// 1. I create the code-behind of the Login page
/// The page is rendered statically (SSR) because login has to set the auth cookie
/// </summary>
[Route(AppRoutes.Login)]
public partial class Login
{
    // 2. ViewModel with the authentication logic
    [Inject]
    private AccountViewModel VM { get; set; } = default!;

    // 3. NavigationManager for the redirect after login
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    // 4. Blazor fills this property with the posted form data
    [SupplyParameterFromForm]
    private LoginInput Input { get; set; } = new();

    // 5. Page requested before login (Identity passes it in the query string)
    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    // 6. Called when the form is valid: if login succeeds I go back to the requested page
    private async Task LoginAsync()
    {
        if (await VM.LoginAsync(Input))
        {
            Navigation.NavigateTo(GetSafeReturnUrl(), forceLoad: true);
        }
    }

    // 7. I only accept local URLs, so nobody can use the login page to send users elsewhere
    private string GetSafeReturnUrl()
        => !string.IsNullOrEmpty(ReturnUrl) && ReturnUrl.StartsWith('/') && !ReturnUrl.StartsWith("//")
            ? ReturnUrl
            : AppRoutes.Home;
}
