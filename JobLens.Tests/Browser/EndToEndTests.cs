using Microsoft.Playwright;

namespace JobLens.Tests.Browser;

/// <summary>
/// Full user journeys in a real browser against the running container.
/// Run only when a site is up (see docs/TESTING.md), so they carry the E2E category.
/// </summary>
[Trait("Category", "E2E")]
public class EndToEndTests : IClassFixture<BrowserFixture>
{
    private readonly BrowserFixture _browser;

    public EndToEndTests(BrowserFixture browser) => _browser = browser;

    [Fact]
    public async Task TheDashboardGreetsTheUserAndOffersToLogAnApplication()
    {
        var page = await _browser.NewPageAsync();

        await page.GotoAsync("/");

        await Assertions.Expect(page.Locator(".welcome-title")).ToBeVisibleAsync();
        await Assertions.Expect(page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Log an application" }))
            .ToBeVisibleAsync();
    }

    [Fact]
    public async Task ANewApplicationAppearsInTheListWithItsStatus()
    {
        var page = await _browser.NewPageAsync();
        var employer = UiSteps.NewName("E2E");
        try
        {
            await UiSteps.CreateApplicationAsync(page, employer, "Test Engineer", "Interview");

            var row = UiSteps.Row(page, employer);
            await Assertions.Expect(row).ToBeVisibleAsync();
            await Assertions.Expect(row.Locator(".badge")).ToHaveTextAsync("Interview");
        }
        finally
        {
            await UiSteps.DeleteMatchingAsync(page, employer);
        }
    }

    [Fact]
    public async Task SearchingFindsOnlyTheMatchingApplication()
    {
        var page = await _browser.NewPageAsync();
        var tag = UiSteps.NewName("Search");
        try
        {
            await UiSteps.CreateApplicationAsync(page, $"{tag}-Keep", "Developer");
            await UiSteps.CreateApplicationAsync(page, $"{tag}-Other", "Developer");

            await UiSteps.SearchAsync(page, $"{tag}-Keep");

            await Assertions.Expect(UiSteps.Row(page, $"{tag}-Keep")).ToBeVisibleAsync();
            await Assertions.Expect(UiSteps.Row(page, $"{tag}-Other")).ToHaveCountAsync(0);
        }
        finally
        {
            await UiSteps.DeleteMatchingAsync(page, tag);
        }
    }

    [Fact]
    public async Task SavingWithoutAnEmployerShowsAHelpfulMessage()
    {
        var page = await _browser.NewPageAsync();

        await page.GotoAsync("/Applications/Create");
        await page.Locator("#Role").FillAsync("Developer");
        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save application" }).ClickAsync();

        await Assertions.Expect(page.Locator(".field-error", new PageLocatorOptions { HasText = "Add the employer" }))
            .ToBeVisibleAsync();
        Assert.Contains("/Applications/Create", page.Url);
    }

    [Fact]
    public async Task TheVersionPageShowsTheCommitThatWasBuiltIn()
    {
        var page = await _browser.NewPageAsync();

        await page.GotoAsync("/Version");

        var commit = await page.Locator("dd code").InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(commit));
        Assert.NotEqual("local-build", commit);   // would mean BUILD_COMMIT never reached the container
    }
}
