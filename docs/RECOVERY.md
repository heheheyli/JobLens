# Recovery procedure: rolling back a bad release

**Goal:** get an environment back onto the last good version in under 10 minutes.

## Why this works

Every image in GHCR is tagged with the full ID of the commit that built it, and tags are never
reused. The pipeline deploys by image tag, so putting an earlier tag back on the app is the
whole rollback. The Version page shows which commit is running.

## When to roll back

Roll back first and investigate afterwards when production is down or clearly broken, for example:

- `/health` fails, or the site shows errors.
- A monitoring alert fires (CPU or memory over 80%, error rate over 1%, or the availability check fails)
  right after a release.
- A change reached production that should not have.

## Steps

1. **Find the last good commit ID.** Open the repository's **Actions** tab and look at the CI
   runs on `main`. Pick the most recent run where `deploy-production` is green and which ran
   *before* the bad release. Click the run, copy the full commit ID from the header (40 characters).
   You can also check the **Packages** page: the tag names are the commit IDs.
2. **Start the rollback.** Actions tab, **Rollback** workflow, **Run workflow**. Choose `production`
   (or `staging`) and paste the commit ID, then run it.
3. **Wait for the green tick.** The workflow redeploys that image, waits for `/health`, and checks
   that the Version page shows the commit ID you asked for. The run summary prints how many seconds
   the rollback took.
4. **Check it yourself.** Open the production site, then the Version page, and confirm the commit.
5. **Stop it happening again.** On GitHub, open the pull request that caused the problem and click
   **Revert**, then merge the revert pull request. If you skip this, the next merge to `main`
   deploys the bad change again.

## Things to know

- A rollback changes the app version only. Database migrations already applied are not undone, so
  keep migrations additive (new columns and tables, no dropping or renaming) so that an older
  version still works against the newer database.
- Staging and production are rolled back separately.
- If GitHub Actions is down, the same rollback by hand is:
  `az containerapp update --name joblens-production --resource-group rg-joblens --image ghcr.io/heheheyli/joblens:<commit ID>`

## Timed rollback test (evidence for the report)

Do this once before the demo and keep the screenshots.

1. Note the commit ID currently on production (Version page). Call it **A**.
2. Deploy a later commit **B** through the normal pipeline and confirm the Version page shows B.
3. Start a stopwatch and run the **Rollback** workflow with commit **A**.
4. Stop the stopwatch when the Version page shows A again.
5. Record the time in the table below. The target is under 10 minutes.

| Date | From | To | Stopwatch time | Workflow time | Passed (under 10 min) |
| --- | --- | --- | --- | --- | --- |
|  |  |  |  |  |  |
