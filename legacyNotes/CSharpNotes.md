dotnet new console -n HelloWorld
dotnet new console -n HelloWorld --use-program-main

dotnet sln add HelloWorld/HelloWorld.csproj
dotnet sln list

dotnet run

dotnet new webapp -n HelloRazor

dotnet new web -n gameStore.Api

https://www.youtube.com/watch?v=YbRe4iIVYJk
### init project
dotnet new list
dotnet new web -n GameStore.api
ή 
ctrl+shift+p → .net:New Project → core web api ή core empty

### build 
right click project solution explorer → build
ή 
dotnet build

### run
f5
ή 
solution explorer → debug → start without debugging
ή 
dotnet run

## REST
### create dto class 
solution → new file → record

```c#
namespace GameStore.Api.Dtos;

public record GameDto(
  int Id,
  string Name,
  string Genre,
  decimal Price,
  DateOnly ReleaseDate
);
```

και το dto το χρησιμοποιώ στο program.cs
```c#
List<GameDto> games = [
  new(
    1,
    "street fighter II",
    "Fighting",
    19.99M,
    new DateOnly(1992, 7,15)
  ),
  new(
    2,
    "Final Fantasy VII Rebirth",
    "RPG",
    69.99M,
    new DateOnly(2024, 2, 29)
  ),
  new(
    3,
    "Astro Bot",
    "Platformer",
    59.99M,
    new DateOnly(2024, 9, 6)
  )
];
```

### endpoint παράδειγμα
```c#
    group.MapPost("/", (CreateGameDto newGame) =>
    {
      GameDto game = new(
        games.Count + 1,
        newGame.Name,
        newGame.Genre,
        newGame.Price,
        newGame.ReleaseDate
      );

      games.Add(game);

      return Results.CreatedAtRoute(GetGameEndpointName, new {id = game.Id}, game);
    });
```
προσοχή, για να μπορέσω να κάνω το new θα πρέπει να φτιάξω και αρχείο κλάσης dto

### validation
μπορώ στην κλάση dto
```c#
[Required] string Name,
```
αλλα στο app
```c#
builder.Services.AddValidation();
```

### Προσθήκη nuget library (→ entity orm)
Microsoft.EntityFrameworkCore.Sqlite

solution → add nuGet → Microsoft.EntityFrameworkCore.Sqlite

### συνδεση με sql
- φτιάχνω φακελο data και κλάση για context (κάτι σαν το mongoose model)
- η κλάση inherit from : DbContext
- δίνω στο DbContext options:
`public class GamesStoreContext(DbContextOptions<GamesStoreContext> options) : DbContext(options)`
```c#
// GameStore.Api\Data\GamesStoreContext.cs
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public class GamesStoreContext(DbContextOptions<GamesStoreContext> options) : DbContext(options)
{
  public DbSet<Game> Games => Set<Game>();
  public DbSet<Genre> Genres => Set<Genre>();
}
```
και μετά πρέπει να την αρχικοποιήσω στο program.cs
```c#
var connString = "Data Source=GameStore.db";
builder.Services.AddSqlite<GamesStoreContext>(connString);
```

### migration
- χρειαζόμαστε την βιβλιοθήκη dotnet-ef
`dotnet tool install --global dotnet-ef --version 11.0.0-preview.3.26207.106`
αυτό εγκαταστάθηκε global
μετα solution→get nuget→EntityFrameworkCore.design

- στο τέρμιναλ τρέχουμε το αρχικό migration. βάζουμε και που θα δημιουργηθεί
`dotnet ef migrations add InitialCreate --output-dir Data\Migrations` 
αυτο μου εφτιαξε τα migrations αρχεία και τωρα πρέπει να τα τρέξουμε
`dotnet ef database update`
και μου δημιουργησε το .db αρχείο που ζητάγαμε εδω `var connString = "Data Source=GameStore.db";`

> ### Προσθήκη δεύτερου table (νέο DbContext ή νέο Model)
> Όταν προσθέτω νέο table (π.χ. Notes):
> 1. **Φτιάχνω Model + DbSet**
> ```csharp
> public DbSet<Note> Notes => Set<Note>();
> ```
> 2. **Κάνω migration για το συγκεκριμένο context**
```bash
dotnet ef migrations add InitialNotes \
  --context NotesContext \
  --output-dir Data/Migrations/Notes
```
>
> 3. **Εφαρμόζω το migration**
 ```bash
 dotnet ef database update --context NotesContext
 ```
> Σημαντικό:
> - Κάθε νέο DbContext χρειάζεται δικό του migration
> - Αν δεν κάνω migration → δεν δημιουργείται table

- για να δω την db με sqlite extension
- για να μην τρέχω κάθε φορα `dotnet ef database update` το βάζω στον κώδικα φτιαχνω /Data/DataExtensions κλάση
```c#
public static class DataExtensions
{
  // το WebApplication είναι αυτό που μου επιτρεπει τα app.MigrateDb();
  public static void MigrateDb(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GamesStoreContext>();
    dbContext.Database.Migrate();
  }
}
```
και το βάζουμε στο program.cs
`app.MigrateDb();`

- λιγότερα Logs
appsettings.json → `"Microsoft.EntityFrameworkCore.Database.Command": "Warning"`

### αρχικοποιηση db (seeding)
αλλαζω το 
```c#
var connString = "Data Source=GameStore.db";
builder.Services.AddSqlite<GamesStoreContext>(connString);
```
σε
```c#
var connString = "Data Source=GameStore.db";
builder.Services.AddSqlite<GamesStoreContext>(
  connString,
  optionsAction: Options => Options.UseSeeding((ContextBoundObject, _) =>
  {
    if(!ContextBoundObject.Set<Genre>().Any())
    {
      ContextBoundObject.Set<Genre>().AddRange(
        new Genre {Name = "Fighting"},
        new Genre {Name = "RPG"},
        new Genre {Name = "Platformer"},
        new Genre {Name = "Racing"},
        new Genre {Name = "Sports"}
      );
      ContextBoundObject.SaveChanges();
    }
  })
);
```

## appsetings.json → IConfiguration
`var connString = builder.Configuration.GetConnectionString("GameStore");`
το αντιστοιχο σε Node `const connString = process.env.DB_CONNECTION`

δεν κατάλαβα ακριβώς αλλα δεν πειραζει. είναι για να ορισουμε διαφορετική μεταβλητη στο appsetings on execute

PS D:\coding\CSharp\GameStore.Api> $env:ConnectionStrings__GameStore="Data Source=Production.db"
PS D:\coding\CSharp\GameStore.Api> dotnet run

## dependency injection
θέλω πχ να κάνω κατι log
public MyService()
{
  var logger = new LoggerApp()
  logger("log this")  
}

τι γίνετε όμως αν η LoggerApp αλλάξει και θέλει
var logger = new LoggerApp(fileForKeepingLogsAlso)
τοτε θα έπρεπε να αλλάξω ξανά τον κωδικά μου
μπορώ ομως να κάνω αυτό:
private readonly LoggerApp _logger;
public MyService(LoggerApp logger)
{
  logger("log this")  
}
Που γίνετε ομως το instantiate → IServiceProvider


### dependency injection στο post
αλλαζουμε απο 
```c#
    group.MapPost("/", (CreateGameDto newGame) =>
    {
      GameDto game = new(
        games.Count + 1,
        newGame.Name,
        newGame.Genre,
        newGame.Price,
        newGame.ReleaseDate
      );

      games.Add(game);

      return Results.CreatedAtRoute(GetGameEndpointName, new {id = game.Id}, game);
    });

```
σε
```c#
    // POST /games
    group.MapPost("/", (CreateGameDto newGame, GamesStoreContext dbContext) =>
    {
      Game game = new()
      {
        Name = newGame.Name,
        GenreId = newGame.GenreId,
        Price = newGame.Price,
        ReleaseDate = newGame.ReleaseDate
      };

      dbContext.Games.Add(game);
      dbContext.SaveChanges();

      GameDetailsDto gameDto = new(
        game.Id,
        game.Name,
        game.GenreId,
        game.Price,
        game.ReleaseDate
      );

      return Results.CreatedAtRoute(GetGameEndpointName, new {id = gameDto.Id}, gameDto);
    });

```
δηλ βάλαμε και το GamesStoreContext dbContext στα params

και στο GameStore.Api\Data\DataExtensions.cs προσθέτουμε
builder.Services.AddScoped<GamesStoreContext>();
και έπρεπε να αλλάξουμε ολα τα dto να παιρνουν genreId αντι για string Genre

## async