using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Data.InMemory;
using TaskManagementApp.Models;

namespace TaskManagementApp.Data.Seed;

/// <summary>
/// 1. I create the seeder that reads Data/Seed/task-data.json at startup
/// The same JSON feeds both modes:
///  - UseDatabase = true  -> I insert the data into PostgreSQL (only if it's missing)
///  - UseDatabase = false -> I load the data into the dictionaries of InMemoryDataStore
/// </summary>
public static class DemoDataSeeder
{
    // 2. JSON options: camelCase names and enums written as text ("InProgress")
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task SeedAsync(IServiceProvider services, DataSettings settings, string contentRootPath)
    {
        // 3. I read the JSON file from the project folder
        var path = Path.Combine(contentRootPath, "Data", "Seed", "task-data.json");
        var json = await File.ReadAllTextAsync(path);
        var seed = JsonSerializer.Deserialize<TaskSeedData>(json, JsonOptions)
                   ?? throw new InvalidOperationException("task-data.json is empty or invalid.");

        // 4. The demo user always goes through Identity (PostgreSQL or EF in-memory)
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var (demoUser, isNewUser) = await EnsureDemoUserAsync(userManager, seed.DemoUser);

        if (settings.UseDatabase)
        {
            await SeedDatabaseAsync(services.GetRequiredService<AppDbContext>(), seed, demoUser.Id, isNewUser);
        }
        else
        {
            SeedInMemory(services.GetRequiredService<InMemoryDataStore>(), seed, demoUser.Id);
        }
    }

    // 5. I create the demo user if it doesn't exist yet
    private static async Task<(ApplicationUser User, bool IsNew)> EnsureDemoUserAsync(
        UserManager<ApplicationUser> userManager, SeedUser seedUser)
    {
        var existing = await userManager.FindByEmailAsync(seedUser.Email);
        if (existing is not null)
        {
            return (existing, false);
        }

        var user = new ApplicationUser
        {
            UserName = seedUser.Email,
            Email = seedUser.Email,
            EmailConfirmed = true,
            FullName = seedUser.FullName
        };

        var result = await userManager.CreateAsync(user, seedUser.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Could not create the demo user: {errors}");
        }

        return (user, true);
    }

    // 6. PostgreSQL: I add the missing categories, and the demo tasks only the first time the demo user is created
    private static async Task SeedDatabaseAsync(AppDbContext db, TaskSeedData seed, string demoUserId, bool isNewUser)
    {
        var existingNames = await db.Categories
            .Where(c => c.UserId == null)
            .Select(c => c.Name)
            .ToListAsync();

        foreach (var category in seed.Categories.Where(c => !existingNames.Contains(c.Name)))
        {
            db.Categories.Add(new Category { Name = category.Name, Color = category.Color, Description = category.Description });
        }

        await db.SaveChangesAsync();

        if (!isNewUser)
        {
            return;
        }

        var categoryIds = await db.Categories
            .Where(c => c.UserId == null)
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        db.Tasks.AddRange(seed.Tasks.Select(t => ToTask(t, categoryIds, demoUserId)));
        await db.SaveChangesAsync();
    }

    // 7. In-memory: I fill the dictionaries and assign the Ids with the store counters
    private static void SeedInMemory(InMemoryDataStore store, TaskSeedData seed, string demoUserId)
    {
        foreach (var seedCategory in seed.Categories)
        {
            var category = new Category
            {
                Id = store.NextCategoryId(),
                Name = seedCategory.Name,
                Color = seedCategory.Color,
                Description = seedCategory.Description
            };
            store.Categories[category.Id] = category;
        }

        var categoryIds = store.Categories.Values.ToDictionary(c => c.Name, c => c.Id);

        foreach (var seedTask in seed.Tasks)
        {
            var task = ToTask(seedTask, categoryIds, demoUserId);
            task.Id = store.NextTaskId();
            store.Tasks[task.Id] = task;
        }
    }

    // 8. I convert a JSON task into an entity, turning the relative days into real UTC dates
    private static TaskItem ToTask(SeedTask seedTask, Dictionary<string, int> categoryIds, string userId)
    {
        var today = DateTime.UtcNow.Date;
        var createdAt = DateTime.UtcNow.AddDays(-seedTask.CreatedDaysAgo);

        return new TaskItem
        {
            Title = seedTask.Title,
            Description = seedTask.Description,
            Status = seedTask.Status,
            Priority = seedTask.Priority,
            DueDate = seedTask.DueInDays.HasValue ? today.AddDays(seedTask.DueInDays.Value) : null,
            CategoryId = categoryIds.GetValueOrDefault(seedTask.Category, categoryIds.Values.First()),
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }
}
