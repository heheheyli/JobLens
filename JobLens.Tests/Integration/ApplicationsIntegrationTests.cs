using System.Net;
using JobLens.Web.Data;
using JobLens.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobLens.Tests.Integration;

/// <summary>
/// The list page searches with PostgreSQL's ILIKE, which an in-memory fake database cannot do,
/// so these tests run against a real PostgreSQL container.
/// </summary>
[Collection(IntegrationCollection.Name)]
public class ApplicationsIntegrationTests
{
    private readonly IntegrationFixture _fixture;

    public ApplicationsIntegrationTests(IntegrationFixture fixture) => _fixture = fixture;

    private static string NewTag() => Guid.NewGuid().ToString("N")[..8];

    private static string Today => DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

    private static JobApplication NewApp(string employer, string role, ApplicationStatus status) => new()
    {
        Employer = employer,
        Role = role,
        Status = status,
        DateApplied = DateOnly.FromDateTime(DateTime.Today),
    };

    private async Task SeedAsync(params JobApplication[] apps)
    {
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobLensDbContext>();
        db.JobApplications.AddRange(apps);
        await db.SaveChangesAsync();
    }

    private async Task<JobApplication?> FindAsync(int id)
    {
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobLensDbContext>();
        return await db.JobApplications.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
    }

    private HttpClient NoRedirectClient() =>
        _fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task List_FiltersByStatusAndSearchTerm()
    {
        var tag = NewTag();
        await SeedAsync(
            NewApp($"{tag}-Alpha", "Engineer", ApplicationStatus.Interview),
            NewApp($"{tag}-Bravo", "Engineer", ApplicationStatus.Applied),
            NewApp($"{tag}-Charlie", "Designer", ApplicationStatus.Interview));
        var client = _fixture.Factory.CreateClient();

        var html = await client.GetStringAsync($"/Applications?employer={tag}&status=Interview");

        Assert.Contains($"{tag}-Alpha", html);
        Assert.Contains($"{tag}-Charlie", html);
        Assert.DoesNotContain($"{tag}-Bravo", html);
    }

    [Fact]
    public async Task Search_IgnoresUpperAndLowerCase()
    {
        var tag = NewTag();
        await SeedAsync(NewApp($"{tag}-Acme", "Engineer", ApplicationStatus.Applied));
        var client = _fixture.Factory.CreateClient();

        var html = await client.GetStringAsync($"/Applications?employer={tag.ToUpperInvariant()}");

        Assert.Contains($"{tag}-Acme", html);
    }

    [Fact]
    public async Task Search_AlsoMatchesTheJobTitle()
    {
        var tag = NewTag();
        await SeedAsync(NewApp("Some Employer", $"{tag}-Wizard", ApplicationStatus.Applied));
        var client = _fixture.Factory.CreateClient();

        var html = await client.GetStringAsync($"/Applications?employer={tag}");

        Assert.Contains($"{tag}-Wizard", html);
    }

    [Fact]
    public async Task List_CanBeSortedByEmployerAToZ()
    {
        var tag = NewTag();
        await SeedAsync(
            NewApp($"{tag}-Charlie", "Engineer", ApplicationStatus.Applied),
            NewApp($"{tag}-Alpha", "Engineer", ApplicationStatus.Applied),
            NewApp($"{tag}-Bravo", "Engineer", ApplicationStatus.Applied));
        var client = _fixture.Factory.CreateClient();

        var html = await client.GetStringAsync($"/Applications?employer={tag}&sort=employer");

        var alpha = html.IndexOf($"{tag}-Alpha", StringComparison.Ordinal);
        var bravo = html.IndexOf($"{tag}-Bravo", StringComparison.Ordinal);
        var charlie = html.IndexOf($"{tag}-Charlie", StringComparison.Ordinal);
        Assert.True(alpha >= 0 && alpha < bravo && bravo < charlie,
            $"Expected Alpha < Bravo < Charlie but found {alpha}, {bravo}, {charlie}");
    }

    [Fact]
    public async Task Create_SavesTheApplicationInTheDatabase()
    {
        var employer = $"Create-{NewTag()}";
        var client = NoRedirectClient();
        var token = await FormHelper.GetTokenAsync(client, "/Applications/Create");

        var response = await client.PostAsync("/Applications/Create", FormHelper.Form(token,
            ("Employer", employer), ("Role", "Platform Engineer"), ("DateApplied", Today), ("Status", "Applied")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobLensDbContext>();
        var saved = await db.JobApplications.SingleAsync(a => a.Employer == employer);
        Assert.Equal("Platform Engineer", saved.Role);
        Assert.Equal(ApplicationStatus.Applied, saved.Status);
    }

    [Fact]
    public async Task Create_WithoutAnEmployer_ShowsAnErrorAndSavesNothing()
    {
        var role = $"NoEmployer-{NewTag()}";
        var client = NoRedirectClient();
        var token = await FormHelper.GetTokenAsync(client, "/Applications/Create");

        var response = await client.PostAsync("/Applications/Create", FormHelper.Form(token,
            ("Role", role), ("DateApplied", Today), ("Status", "Applied")));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Add the employer", await response.Content.ReadAsStringAsync());
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobLensDbContext>();
        Assert.False(await db.JobApplications.AnyAsync(a => a.Role == role));
    }

    [Fact]
    public async Task Edit_UpdatesTheSavedApplication()
    {
        var app = NewApp($"Edit-{NewTag()}", "Junior Developer", ApplicationStatus.Applied);
        await SeedAsync(app);
        var client = NoRedirectClient();
        var token = await FormHelper.GetTokenAsync(client, $"/Applications/Edit/{app.Id}");

        var response = await client.PostAsync($"/Applications/Edit/{app.Id}", FormHelper.Form(token,
            ("Id", app.Id.ToString()), ("Employer", app.Employer), ("Role", "Senior Developer"),
            ("DateApplied", Today), ("Status", "Interview")));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var updated = await FindAsync(app.Id);
        Assert.Equal("Senior Developer", updated!.Role);
        Assert.Equal(ApplicationStatus.Interview, updated.Status);
    }

    [Fact]
    public async Task Delete_RemovesTheApplication()
    {
        var app = NewApp($"Delete-{NewTag()}", "Engineer", ApplicationStatus.Rejected);
        await SeedAsync(app);
        var client = NoRedirectClient();
        var token = await FormHelper.GetTokenAsync(client, "/Applications");

        var response = await client.PostAsync($"/Applications/Delete/{app.Id}",
            FormHelper.Form(token, ("Id", app.Id.ToString())));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Null(await FindAsync(app.Id));
    }

    [Fact]
    public async Task Details_ForAnApplicationThatDoesNotExist_IsNotFound()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/Applications/Details/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
