using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace JobLens.Tests.Browser;

/// <summary>The things a person does in the browser, reused by the end-to-end and acceptance tests.</summary>
public static class UiSteps
{
    private static readonly Regex ApplicationsList = new("/Applications/?$");

    public static string NewName(string prefix) => $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}";

    public static async Task CreateApplicationAsync(IPage page, string employer, string role, string? status = null)
    {
        await page.GotoAsync("/Applications/Create");
        await page.Locator("#Employer").FillAsync(employer);
        await page.Locator("#Role").FillAsync(role);
        if (status is not null)
        {
            await page.Locator("label.status-option", new PageLocatorOptions { HasText = status }).ClickAsync();
        }

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save application" }).ClickAsync();
        await page.WaitForURLAsync(ApplicationsList);
    }

    public static async Task SearchAsync(IPage page, string term)
    {
        await page.GotoAsync("/Applications");
        var search = page.Locator("input[name=employer]");
        await search.FillAsync(term);
        await search.PressAsync("Enter");
        await page.WaitForURLAsync(new Regex("employer="));
    }

    /// <summary>Deletes every application that matches the search term, so tests leave no data behind.</summary>
    public static async Task DeleteMatchingAsync(IPage page, string term)
    {
        await SearchAsync(page, term);

        var links = page.Locator("a.detail-delete");
        var hrefs = new List<string>();
        var count = await links.CountAsync();
        for (var i = 0; i < count; i++)
        {
            hrefs.Add((await links.Nth(i).GetAttributeAsync("href"))!);
        }

        foreach (var href in hrefs)
        {
            await page.GotoAsync(href);
            await page.Locator("input[value=Delete]").ClickAsync();
            await page.WaitForURLAsync(ApplicationsList);
        }
    }

    public static ILocator Row(IPage page, string text) =>
        page.Locator(".app-row", new PageLocatorOptions { HasText = text });
}
