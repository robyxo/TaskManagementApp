# Architettura: Task Management App 📐

> Documentazione in italiano. Il codice, l'interfaccia e il README sono in inglese.

## Panoramica

Gestione attività con Blazor Server (.NET 9), Entity Framework Core 9 e ASP.NET Core Identity.
Architettura MVVM con repository pattern e due sorgenti dati intercambiabili: PostgreSQL oppure dizionari in memoria caricati da un file JSON.

---

## 1. Come è organizzato il codice

```
Pagina .razor (solo UI)
   ▼  @inherits ViewModelPageBase<XxxViewModel>
Code-behind .razor.cs (rotta da AppRoutes + [Authorize] + gestori eventi)
   ▼
ViewModel (eredita BaseViewModel)
   ▼
ITaskRepository
   ├── EfTaskRepository        → PostgreSQL (EF Core)
   └── InMemoryTaskRepository  → dizionari (InMemoryDataStore)
```

### Regole che seguo

1. Nel `.razor` metto solo markup; rotta, autorizzazione e gestori vanno nel `.razor.cs`.
2. Le rotte le prendo sempre da `Utils/AppRoutes.cs`.
3. Ogni pagina ha il suo ViewModel che eredita da `BaseViewModel`.
4. I ViewModel non conoscono il database: parlano solo con `ITaskRepository`.
5. Commenti in prima persona, divisi in sezioni numerate (nel codice sono in inglese).

---

## 2. Database vero o dati di prova

In `appsettings.json`:

```json
"DataSettings": { "UseDatabase": false }
```

| UseDatabase | Task e categorie | Utenti (Identity) |
|---|---|---|
| `false` | `InMemoryTaskRepository` → `InMemoryDataStore` (ConcurrentDictionary) | provider EF Core In-Memory |
| `true` | `EfTaskRepository` → PostgreSQL | PostgreSQL |

In `Program.cs` leggo la sezione e registro l'implementazione giusta:

```csharp
if (dataSettings.UseDatabase)
    builder.Services.AddScoped<ITaskRepository, EfTaskRepository>();
else
    builder.Services.AddScoped<ITaskRepository, InMemoryTaskRepository>();
```

Identity ha comunque bisogno di EF Core: in modalità demo uso il provider In-Memory, così non serve installare niente.

### Dati iniziali: `Data/Seed/task-data.json`

Lo stesso JSON alimenta entrambe le modalità (`DemoDataSeeder`):
- con PostgreSQL inserisco categorie e task solo se mancano;
- in memoria riempio i dizionari a ogni avvio.

Le date nel JSON sono relative (`dueInDays`, `createdDaysAgo`): le trasformo in date reali all'avvio, così i dati sembrano sempre recenti.

Utente demo: **demo@taskmanager.local / Demo123!**

---

## 3. ViewModelPageBase

Classe base generica per le pagine: inietta il ViewModel, passa l'Id dell'utente loggato e ridisegna la pagina quando il ViewModel notifica `PropertyChanged`.

```razor
@inherits ViewModelPageBase<TasksViewModel>
@rendermode InteractiveServer
```

---

## 4. Perché IDbContextFactory

In Blazor Server un `DbContext` condiviso non regge due query nello stesso momento.
Con la factory ogni metodo di `EfTaskRepository` apre e chiude il suo contesto:

```csharp
await using var db = await _dbFactory.CreateDbContextAsync();
```

---

## 5. Flusso: salvataggio di una task

1. **Modifica** → `VM.OpenEditForm(task)` crea una **copia** della task (Annulla non tocca la tabella).
2. **Salva** → `EditForm` valida con le DataAnnotations e chiama `VM.SaveTaskAsync()`.
3. Il ViewModel converte la data in UTC, imposta l'utente e chiama `_repository.AddAsync` o `UpdateAsync`.
4. Il repository (PostgreSQL o dizionari) salva e controlla che la task appartenga all'utente.
5. Il ViewModel mostra il messaggio e ricarica l'elenco; la pagina si aggiorna con `PropertyChanged`.

---

## 6. Sicurezza

- Login e registrazione in modalità statica (SSR) perché Identity deve scrivere il cookie.
- Ogni query filtra per `UserId`: un utente vede e modifica solo le sue task.
- Dopo il login accetto solo URL interni (protezione open redirect).
- Logout con POST e token antiforgery.
