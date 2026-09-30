# Setup: Task Management App 🚀

## Prerequisites

- .NET 9 SDK
- Visual Studio 2022 (17.12+) or VS Code
- PostgreSQL: **only** if you want to use the real database

---

## Option A: Demo data (no database)

1. Open `appsettings.json` and make sure you have:

   ```json
   "DataSettings": { "UseDatabase": false }
   ```

2. Run the app (F5 in Visual Studio or `dotnet run`).
3. Log in with:
   - **Email:** demo@taskmanager.local
   - **Password:** Demo123!

The data comes from `Data/Seed/task-data.json` and lives in memory: every restart starts again from the JSON.
You can edit the JSON to change the demo user, categories and tasks.

---

## Option B: PostgreSQL

1. Start PostgreSQL, locally or with Docker:

   ```bash
   docker run --name postgres-tasks -e POSTGRES_PASSWORD=postgres123 -p 5432:5432 -d postgres:16
   ```

2. Check the connection string in `appsettings.json`:

   ```json
   "ConnectionStrings": {
     "Postgres": "Host=localhost;Port=5432;Database=taskmanagement;Username=postgres;Password=postgres123;Sslmode=Disable"
   }
   ```

3. Set `"UseDatabase": true` and run the app.

At the first start the app creates the database schema, the demo user and the demo data from the same JSON file.

---

## Troubleshooting

| Problem | Fix |
|---|---|
| `Connection refused` | PostgreSQL is not running, or use `UseDatabase: false` |
| `password authentication failed` | Check user and password in the connection string |
| Old data in another language | Drop the `taskmanagement` database and restart the app |
| Visual Studio stops on `NavigationException` after login | Press Continue: it's how Blazor performs the redirect |
