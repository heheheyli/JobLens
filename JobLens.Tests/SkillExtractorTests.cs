using JobLens.Web.Services;

namespace JobLens.Tests;

public class SkillExtractorTests
{
    [Fact]
    public async Task TheUnconfiguredExtractorExplainsHowToConnectAzure()
    {
        ISkillExtractor extractor = new UnconfiguredSkillExtractor();

        var error = await Assert.ThrowsAsync<SkillExtractorNotConfiguredException>(
            () => extractor.ExtractAsync("Any job ad"));

        Assert.Contains("AzureLanguage:Endpoint", error.Message);
    }
}
