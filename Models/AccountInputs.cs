using System.ComponentModel.DataAnnotations;

namespace TaskManagementApp.Models;

/// <summary>
/// 1. I create the login form model, validated on the server with DataAnnotations
/// </summary>
public class LoginInput
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

/// <summary>
/// 2. I create the registration form model
/// With [Compare] I check that both passwords match
/// </summary>
public class RegisterInput
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
