using Microsoft.Playwright;

namespace JobLens.Tests.Browser;

/// <summary>
/// One shared Chromium browser. The address of the site under test comes from E2E_BASE_URL
/// (http://localhost:8080 when it is not set, which is the docker compose setup).
/// </summary>
public class BrowserFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public string BaseUrl { get; } =
        (Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:8080").TrimEnd('/');

    /// <summary>The commit the deployed site should be running, when the pipeline tells us.</summary>
    public string? ExpectedCommit { get; } = Environment.GetEnvironmentVariable("EXPECTED_COMMIT");

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        Assertions.SetDefaultExpectTimeout(15_000);
    }

    public async Task<IPage> NewPageAsync()
    {
        var context = await _browser!.NewContextAsync(new BrowserNewContextOptions { BaseURL = BaseUrl });
        return await context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }
}
