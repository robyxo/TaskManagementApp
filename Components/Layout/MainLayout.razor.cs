using Microsoft.AspNetCore.Components;
using TaskManagementApp.Data;

namespace TaskManagementApp.Components.Layout;

/// <summary>
/// 1. I create the code-behind of the layout (the base class LayoutComponentBase is set in the .razor)
/// </summary>
public partial class MainLayout
{
    // 2. I inject the data settings to show which data source is active
    [Inject]
    private DataSettings DataSettings { get; set; } = default!;

    // 3. Green badge for the real database, yellow for the demo data
    private string DataModeBadge => DataSettings.UseDatabase ? "bg-success" : "bg-warning text-dark";
}
