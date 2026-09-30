namespace TaskManagementApp.Data.Seed;

// 1. I create the classes that mirror the structure of task-data.json
// Dates are relative (dueInDays, createdDaysAgo), so the demo data always looks recent

// 2. Root of the JSON file
public class TaskSeedData
{
    public SeedUser DemoUser { get; set; } = new();
    public List<SeedCategory> Categories { get; set; } = new();
    public List<SeedTask> Tasks { get; set; } = new();
}

// 3. Demo account created at startup
public class SeedUser
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

// 4. System category
public class SeedCategory
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#0d6efd";
    public string? Description { get; set; }
}

// 5. Demo task: the category is referenced by name
public class SeedTask
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public Models.TaskStatus Status { get; set; }
    public Models.PriorityLevel Priority { get; set; }
    public int? DueInDays { get; set; }
    public int CreatedDaysAgo { get; set; }
}
