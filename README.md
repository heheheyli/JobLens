# JobLens
Job application tracker with AI-assisted job ad analysis.

Built as a team project for SWE40006 - Software Deployment and Evolution,
Swinburne University of Technology.

**Group 06** --- Thursday 10:30
Hayley Nguyen (105005495) · Numaya Senanayake (105305030)

## Pages

| Page | What it does |
| --- | --- |
| Dashboard | Status counts, response rate, past-week goal, day streak, follow-ups and stale applications (no reply after 14 days) |
| Applications | Add, edit, delete, search, filter and sort applications, with a detail panel |
| Analyser | Pulls skills out of a job ad with Azure AI Language and shows a match score and skill gaps |
| Profile | Your technical, personal and other skills, used by the Analyser |
| Version | Commit and build time of the running app |
| About | About us|

## How to run locally

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Docker Desktop](https://www.docker.com/products/docker-desktop/).

### First time only setup
1. Copy `.env.example` to `.env` and choose a database password.
2. Start the database (Postgres on port 5433)
```
docker compose up -d db
```
3. Tell the app how to reach it. Use the same password put in `.env`
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=joblens;Username=joblens;Password=YOUR_PASSWORD" --project JobLens.Web
```
4. For the Analyser, add the Azure AI Language endpoint and key (Azure portal → the Language resource → Keys and Endpoint). Without these the rest of the app still works, and the Analyser says it isn't connected.
```
dotnet user-secrets set "AzureLanguage:Endpoint" "https://YOUR_RESOURCE.cognitiveservices.azure.com/" --project JobLens.Web
dotnet user-secrets set "AzureLanguage:Key" "YOUR_KEY" --project JobLens.Web
```

### How to run the app

```
docker compose up -d db
dotnet watch --project JobLens.Web
```

Open <http://localhost:5072>. Press **Ctrl+R** in that terminal to restart it if changes are made to the app.

To run the app in Docker the way it's deployed instead, use `docker compose up -d --build` and open <http://localhost:8080>. Rebuild with `--build` after every change, or you'll see the old version. User secrets don't reach the container, so the Analyser isn't connected there.

### Run the tests

```powershell
dotnet test
```

Stop the running app first, or the build can't replace its files.

## Deployment

Merging to `main` runs `.github/workflows/ci.yml`: build and test, push the image to GHCR, deploy it to the `joblens-staging` Container App, health check `/health`, then deploy the same image to `joblens-production`. Database migrations run automatically when the app starts.


## Where things are

| Path | What's in it |
| --- | --- |
| `JobLens.Web/Controllers` | Page logic |
| `JobLens.Web/Models` | Data classes, statuses and skill categories |
| `JobLens.Web/Services` | Testable logic: `DashboardStats` (dashboard numbers), `SkillMatcher` (match score), `DateLabels`, `StatusLabels`, and the skill extractors behind `ISkillExtractor` |
| `JobLens.Web/Views` | All the app pages |
| `JobLens.Web/wwwroot` | `css/site.css` (all styling) and `js/application-form.js` (form helpers) |
| `JobLens.Web/Migrations` | Database migrations, applied automatically on startup |
| `JobLens.Tests` | xUnit tests |
