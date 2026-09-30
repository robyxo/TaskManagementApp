using Microsoft.AspNetCore.Identity;
using TaskManagementApp.Models;

namespace TaskManagementApp.ViewModels;

/// <summary>
/// 1. I create the ViewModel for login and registration
/// I use Identity's SignInManager and UserManager, so the pages stay UI only
/// It works the same way in both data modes, because Identity always goes through EF Core
/// </summary>
public class AccountViewModel : BaseViewModel
{
    // 2. Identity services
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    // 3. With this constructor I get the Identity services so I can call them below
    public AccountViewModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        Title = "Account";
    }

    // 4. Login: I check email and password and create the auth cookie
    public async Task<bool> LoginAsync(LoginInput input)
    {
        ErrorMessage = string.Empty;

        // 5. The UserName is the email (I set it that way on registration)
        var result = await _signInManager.PasswordSignInAsync(
            input.Email, input.Password, input.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ErrorMessage = "Invalid email or password.";
            return false;
        }

        // 6. I store the last login date
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user is not null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return true;
    }

    // 7. Registration: I create the user and sign them in right away
    public async Task<bool> RegisterAsync(RegisterInput input)
    {
        ErrorMessage = string.Empty;

        var user = new ApplicationUser
        {
            UserName = input.Email,
            Email = input.Email,
            FullName = input.FullName
        };

        var result = await _userManager.CreateAsync(user, input.Password);
        if (!result.Succeeded)
        {
            // 8. I show Identity's errors (weak password, email already used, ...)
            ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            return false;
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return true;
    }
}
