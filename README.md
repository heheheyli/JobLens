# Project
Job application tracker with AI-assisted job ad analysis.

Built as a team project for SWE40006 - Software Deployment and Evolution,
Swinburne University of Technology.

**Group 06** --- Thursday 10:30
Hayley Nguyen (105005495) · Numaya Senanayake (105305030)


## How to run locally

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Docker Desktop](https://www.docker.com/products/docker-desktop/).

### First time only setup
1. Copy .env.example to .env and choose a password, start the database, then save the connection string in user secrets.
2. Start the database (Postgres on port 5433)
```
docker compose up -d db
```
3. Tell the app how to reach it. Use the same password put in .env
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=joblens;Username=joblens;Password=YOUR_PASSWORD" --project JobLens.Web
```

### How to run the app

```
docker compose up -d db
dotnet watch --project JobLens.Web
```

Open <http://localhost:5072>. Press **Ctrl+R** in that terminal to restart it if changes are made to the app.


### Run the tests

```powershell
dotnet test
```


## Where things are

| Path | What's in it |
| --- | --- |
| `JobLens.Web/Controllers` | Page logic |
| `JobLens.Web/Models` | Data classes and the status list |
| `JobLens.Web/Services` | Testable logic |
| `JobLens.Web/Views` | All the app pages |
| `JobLens.Web/wwwroot` | `css/site.css` (all styling) and `js/application-form.js` (form helpers) |
| `JobLens.Web/Migrations` | Database migrations, applied automatically on startup |
| `JobLens.Tests` | xUnit tests |
