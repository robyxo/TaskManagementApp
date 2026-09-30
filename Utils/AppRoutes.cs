namespace TaskManagementApp.Utils;

/// <summary>
/// 1. I create a static class with every route of the application
/// This way I never hard-code a route: if one changes, I change it only here
/// </summary>
public static class AppRoutes
{
    // 2. Dashboard - entry point
    public const string Home = "/";

    // 3. Task management
    public const string Tasks = "/tasks";

    // 4. Login page
    public const string Login = "/login";

    // 5. Registration page
    public const string Register = "/register";

    // 6. Logout endpoint (POST, mapped in Program.cs)
    public const string Logout = "/logout";
}
