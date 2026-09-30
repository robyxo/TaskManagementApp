using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using TaskManagementApp.Utils;

namespace TaskManagementApp.Components.Pages;

/// <summary>
/// 1. I create the code-behind of the Home page (dashboard)
/// The base class (ViewModelPageBase) is declared in the .razor with @inherits:
/// it injects the HomeViewModel and loads the data, so here I only need route and authorization
/// </summary>
[Route(AppRoutes.Home)]
[Authorize]
public partial class Home
{
}
