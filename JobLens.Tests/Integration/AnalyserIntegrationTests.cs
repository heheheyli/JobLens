using System.Net;
using JobLens.Web.Data;
using Microsoft.Extensions.DependencyInjection;

namespace JobLens.Tests.Integration;

/// <summary>The Analyser runs with a fake skill extractor that always finds C#, Docker and SQL.</summary>
[Collection(IntegrationCollection.Name)]
public class AnalyserIntegrationTests
{
    private readonly IntegrationFixture _fixture;

    public AnalyserIntegrationTests(IntegrationFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task AnAdIsScoredAgainstTheSavedProfile()
    {
        using (var scope = _fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JobLensDbContext>();
            var profile = await db.GetOrCreateProfileAsync();
            profile.AddSkills(new[] { "C#", "SQL" });
            await db.SaveChangesAsync();
        }

        var client = _fixture.Factory.CreateClient();
        var token = await FormHelper.GetTokenAsync(client, "/Analyser");

        var response = await client.PostAsync("/Analyser", FormHelper.Form(token,
            ("AdText", "We are looking for someone with C#, Docker and SQL.")));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("67%", html);
        Assert.Contains("2 of 3 skills match", html);
        Assert.Contains("Docker", html);
    }

    [Fact]
    public async Task AnEmptyAdAsksTheUserToPasteOne()
    {
        var client = _fixture.Factory.CreateClient();
        var token = await FormHelper.GetTokenAsync(client, "/Analyser");

        var response = await client.PostAsync("/Analyser", FormHelper.Form(token, ("AdText", "")));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Paste a job ad first", await response.Content.ReadAsStringAsync());
    }
}
