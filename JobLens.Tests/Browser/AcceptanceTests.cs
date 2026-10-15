using Microsoft.Playwright;

namespace JobLens.Tests.Browser;

/// <summary>
/// System and acceptance checks written as user stories. The pipeline runs them against the
/// staging site after each deployment, and production is only released if they all pass.
/// E2E_BASE_URL is the staging address and EXPECTED_COMMIT is the commit being released.
/// </summary>
[Trait("Category", "Acceptance")]
public class AcceptanceTests : IClassFixture<BrowserFixture>
{
    private readonly BrowserFixture _browser;

    public AcceptanceTests(BrowserFixture browser) => _browser = browser;

    [Fact]
    public async Task AsAnOperator_IWantTheServiceToReportItselfHealthy()
    {
        var page = await _browser.NewPageAsync();

        var response = await page.GotoAsync("/health");

        Assert.True(response!.Ok);
        await Assertions.Expect(page.Locator("body")).ToHaveTextAsync("Healthy");
    }

    [Fact]
    public async Task AsAnOperator_IWantToSeeThatTheDeployedVersionIsTheCommitWeJustReleased()
    {
        var page = await _browser.NewPageAsync();

        await page.GotoAsync("/Version");
        var commit = await page.Locator("dd code").InnerTextAsync();

        Assert.False(string.IsNullOrWhiteSpace(commit));
        Assert.NotEqual("local-build", commit);
        if (!string.IsNullOrWhiteSpace(_browser.ExpectedCommit))
        {
            Assert.Equal(_browser.ExpectedCommit, commit);
        }
    }

    [Fact]
    public async Task AsAJobSeeker_IWantToLogAnApplicationAndFindItAgain()
    {
        var page = await _browser.NewPageAsync();
        var employer = UiSteps.NewName("ACC");
        try
        {
            await UiSteps.CreateApplicationAsync(page, employer, "Graduate Developer", "Applied");
            await UiSteps.SearchAsync(page, employer);

            var row = UiSteps.Row(page, employer);
            await Assertions.Expect(row).ToBeVisibleAsync();
            await Assertions.Expect(row.Locator(".badge")).ToHaveTextAsync("Applied");
        }
        finally
        {
            await UiSteps.DeleteMatchingAsync(page, employer);
        }
    }

    [Fact]
    public async Task AsAJobSeeker_IWantADashboardThatInvitesMeToLogAnApplication()
    {
        var page = await _browser.NewPageAsync();

        await page.GotoAsync("/");

        await Assertions.Expect(page.Locator(".welcome-title")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Log an application" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task AsAJobSeeker_IWantTheAnalyserToTellMeWhenIForgetToPasteAnAd()
    {
        var page = await _browser.NewPageAsync();

        await page.GotoAsync("/Analyser");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Analyse ad" }).ClickAsync();

        await Assertions.Expect(page.Locator(".analyser-error")).ToContainTextAsync("Paste a job ad first");
    }

    [Fact]
    public async Task AsAJobSeeker_IWantMySkillsProfilePageToLoad()
    {
        var page = await _browser.NewPageAsync();

        var response = await page.GotoAsync("/Profile");

        Assert.True(response!.Ok);
    }
}
