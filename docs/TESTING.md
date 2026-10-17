# Testing

| Layer | What it checks | Where it runs | Command |
| --- | --- | --- | --- |
| Unit | Business rules: dashboard numbers, skill matching, labels, validation | Every push and pull request | `dotnet test --filter "Category!=E2E&Category!=Acceptance"` |
| Integration | The whole app on a real PostgreSQL container: pages, forms, filtering, migrations | Every push and pull request | same command (needs Docker running) |
| End-to-end (`E2E`) | User journeys in a real browser against the docker compose app | `e2e` job on every push and pull request | see below |
| Acceptance (`Acceptance`) | User stories against the live staging site, including "staging runs the commit we are releasing" | `acceptance-staging` job after the staging deploy, before production | see below |

The Azure AI skill extractor is replaced by a fake in the integration tests, so no test calls the real service.

## Run the browser tests on your computer

1. Install the browser once, after building: `pwsh JobLens.Tests/bin/Debug/net10.0/playwright.ps1 install chromium`
2. Start the app: `docker compose up -d --build` (needs a `.env`, see the README).
3. Run them: `E2E_BASE_URL=http://localhost:8080 dotnet test --filter "Category=E2E"`

To run the acceptance tests against another site, set `E2E_BASE_URL` to its address and
`EXPECTED_COMMIT` to the commit it should be running, then use `--filter "Category=Acceptance"`.

## Coverage

The `build-and-test` job fails if line coverage of the business logic (`JobLens.Web.Services` and
`SkillProfileExtensions`) drops below 70%. The HTML report is attached to the run as `coverage-report`.
