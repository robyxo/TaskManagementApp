# Task Management App ✅

A task manager built with **Blazor Server (.NET 9)**, **Entity Framework Core 9** and **ASP.NET Core Identity**, following a clean **MVVM** architecture with a **repository pattern** and two swappable data sources:

- **PostgreSQL** (real database)
- **In-memory dictionaries** filled from a JSON file, so you can run it with no database at all

> Portfolio project. All data is fictional.

---

## ✨ Features

- Sign up, log in and log out (ASP.NET Core Identity, cookie authentication)
- Personal dashboard: totals, tasks in progress, completed tasks, latest tasks
- Full task CRUD with server-side validation (DataAnnotations)
- Filters by status and priority, live search on title and description
- Categories with colors, due dates, priorities
- Each user only sees their own tasks
- **Data source switch** in `appsettings.json`: PostgreSQL or demo data

---

## 🔀 Two Data Modes

```json
"DataSettings": {
  "UseDatabase": false
}
```

| `UseDatabase` | Tasks and categories | Identity (users) | Needs PostgreSQL |
|---|---|---|---|
| `false` (default) | `InMemoryTaskRepository` → dictionaries in `InMemoryDataStore` | EF Core in-memory provider | No |
| `true` | `EfTaskRepository` → PostgreSQL | PostgreSQL | Yes |

In both modes the starting data comes from **`Data/Seed/task-data.json`** (demo user, categories and tasks). Dates in the JSON are relative (`dueInDays`, `createdDaysAgo`), so the demo always looks current.

The ViewModels only depend on `ITaskRepository`. `Program.cs` picks the implementation at startup and nothing else in the app changes.

---

## 🛠️ Tech Stack

| Area | Technology |
|------|------------|
| UI | Blazor Server (Interactive Server), Bootstrap 5, Bootstrap Icons |
| Language | C# 13, .NET 9 |
| Data | EF Core 9 + Npgsql, EF Core In-Memory (Identity in demo mode) |
| Architecture | MVVM (CommunityToolkit.Mvvm), Repository pattern |
| Auth | ASP.NET Core Identity |

---

## 🏗️ Architecture

```
Razor page (.razor = UI only)
   │  @inherits ViewModelPageBase<TViewModel>
   ▼
Code-behind (.razor.cs = route + [Authorize] + event handlers)
   ▼
ViewModel (inherits BaseViewModel : ObservableObject)
   ▼
ITaskRepository
   ├── EfTaskRepository        → IDbContextFactory<AppDbContext> → PostgreSQL
   └── InMemoryTaskRepository  → InMemoryDataStore (ConcurrentDictionary)
```

Key design choices:

- **`ViewModelPageBase<TViewModel>`**: a generic base component that injects the ViewModel, passes the logged-in user and re-renders the page when the ViewModel raises `PropertyChanged`.
- **`IDbContextFactory`**: every repository method opens a short-lived `DbContext`, which is the recommended pattern for Blazor Server.
- **Stale-result protection**: the search box reloads on every keystroke; a version counter drops responses that arrive out of order.
- **Centralized routes** in `Utils/AppRoutes.cs`: no hard-coded URLs in the pages.
- **Static SSR for login/sign-up**, so Identity can write the auth cookie; the other pages are interactive.

---

## 📁 Project Structure

```
TaskManagementApp/
├── Components/
│   ├── App.razor, Routes.razor, _Imports.razor, ViewModelPageBase.cs
│   ├── Layout/MainLayout.razor (+ .razor.cs)
│   └── Pages/
│       ├── Home, Tasks (.razor + .razor.cs)
│       └── Account/Login, Register
├── Data/
│   ├── AppDbContext.cs, DataSettings.cs
│   ├── Repositories/   ITaskRepository, EfTaskRepository, InMemoryTaskRepository
│   ├── InMemory/       InMemoryDataStore
│   └── Seed/           task-data.json, SeedData.cs, DemoDataSeeder.cs
├── Models/             TaskItem, Category, ApplicationUser, AccountInputs, TaskQueries
├── ViewModels/         BaseViewModel, HomeViewModel, TasksViewModel, AccountViewModel
├── Utils/              AppRoutes, TaskDisplay
├── wwwroot/app.css
└── Program.cs
```

---

## 🚀 Getting Started

**Quick start (no database):**

1. Keep `"UseDatabase": false` in `appsettings.json`.
2. Run the project (`dotnet run` or F5 in Visual Studio).
3. Log in with **demo@taskmanager.local / Demo123!**, or create a new account.

**With PostgreSQL:**

1. Start PostgreSQL (for example `docker run --name postgres-tasks -e POSTGRES_PASSWORD=postgres123 -p 5432:5432 -d postgres:16`).
2. Check `ConnectionStrings:Postgres` in `appsettings.json`.
3. Set `"UseDatabase": true` and run. The app creates the database, the demo user and the demo data.

More details in [SETUP.md](./SETUP.md).

---

## 🔮 Possible Next Steps

- EF Core migrations instead of `EnsureCreated`
- Custom categories per user
- Unit tests for the ViewModels using the in-memory repository

---

## 📚 Documentation

Italian architecture notes: [DOCS/ARCHITETTURA.md](./DOCS/ARCHITETTURA.md)

---

## 👨‍💻 Author

**Roberto**: [GitHub](https://github.com/yourusername)

MIT License
