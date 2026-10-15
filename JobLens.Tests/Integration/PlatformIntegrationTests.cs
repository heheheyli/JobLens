using System.Net;
using JobLens.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobLens.Tests.Integration;

[Collection(IntegrationCollection.Name)]
public class PlatformIntegrationTests
{
    private readonly IntegrationFixture _fixture;

    public PlatformIntegrationTests(IntegrationFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task HealthEndpoint_ReportsHealthy()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task VersionPage_ShowsTheCommitPassedInByTheBuild()
    {
        var client = _fixture.Factory.CreateClient();

        var html = await client.GetStringAsync("/Version");

        Assert.Contains(IntegrationFixture.FakeCommit, html);
    }

    [Fact]
    public async Task EveryMigrationIsAppliedWhenTheAppStarts()
    {
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JobLensDbContext>();

        var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();

        Assert.NotEmpty(applied);
        Assert.Equal(db.Database.GetMigrations().ToList(), applied);
    }

    [Fact]
    public async Task Dashboard_LoadsAgainstTheRealDatabase()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Log an application", await response.Content.ReadAsStringAsync());
    }
}
