using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Components;
using TaskManagementApp.Data;
using TaskManagementApp.Data.InMemory;
using TaskManagementApp.Data.Repositories;
using TaskManagementApp.Data.Seed;
using TaskManagementApp.Models;
using TaskManagementApp.Utils;
using TaskManagementApp.ViewModels;

// 1. I create the builder
var builder = WebApplication.CreateBuilder(args);

// 2. I register the Razor Components with Interactive Server mode
// Pages with @rendermode InteractiveServer run on the server through SignalR
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 3. I read the DataSettings section: it decides where the data comes from
var dataSettings = builder.Configuration.GetSection(DataSettings.SectionName).Get<DataSettings>()
                   ?? new DataSettings();
builder.Services.AddSingleton(dataSettings);

if (dataSettings.UseDatabase)
{
    // 4. Real database: EF Core + PostgreSQL for Identity AND for the tasks
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
    builder.Services.AddScoped<ITaskRepository, EfTaskRepository>();
}
else
{
    // 5. Demo mode: tasks in dictionaries (InMemoryDataStore), Identity on the EF in-memory provider
    // Nothing to install: everything is lost when the app stops and reloaded from the JSON at the next start
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseInMemoryDatabase("TaskManagementDemo"));
    builder.Services.AddSingleton<InMemoryDataStore>();
    builder.Services.AddScoped<ITaskRepository, InMemoryTaskRepository>();
}

// 6. Identity needs a "normal" scoped AppDbContext: I create it from the same factory
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// 7. ASP.NET Core Identity with simple password rules for the demo
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 8. I tell the Identity cookie where my login/logout pages are (taken from AppRoutes)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = AppRoutes.Login;
    options.LogoutPath = AppRoutes.Logout;
    options.AccessDeniedPath = AppRoutes.Login;
});

// 9. Authorization and authentication state available in every component
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// 10. ViewModels with Scoped lifetime (in Blazor Server = one per user connection)
builder.Services.AddScoped<HomeViewModel>();
builder.Services.AddScoped<TasksViewModel>();
builder.Services.AddScoped<AccountViewModel>();

// 11. I build the app
var app = builder.Build();

// 12. I prepare the data at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 13. With PostgreSQL I apply the migrations if I have them, otherwise I create the schema from the model
    // (GetMigrations only works on relational providers, so I call it only when UseDatabase is true)
    if (dataSettings.UseDatabase && dbContext.Database.GetMigrations().Any())
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    // 14. Demo user, categories and tasks from Data/Seed/task-data.json
    await DemoDataSeeder.SeedAsync(scope.ServiceProvider, dataSettings, app.Environment.ContentRootPath);
}

// 15. In production I enable HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// 16. Pipeline: HTTPS, authentication, authorization, antiforgery
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// 17. Static files (wwwroot and blazor.web.js) and Razor components
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 18. Logout endpoint: the form in MainLayout posts here
app.MapPost(AppRoutes.Logout, async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect(AppRoutes.Login);
});

// 19. I start the app
app.Run();
