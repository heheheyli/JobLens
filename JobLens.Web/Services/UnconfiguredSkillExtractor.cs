namespace JobLens.Web.Services;

public class SkillExtractorNotConfiguredException()
    : InvalidOperationException("The analyser isn't connected to Azure AI Language yet. Add AzureLanguage:Endpoint and AzureLanguage:Key to the app settings.");

public class UnconfiguredSkillExtractor : ISkillExtractor
{
    public Task<IReadOnlyList<string>> ExtractAsync(string text, CancellationToken ct = default) =>
        throw new SkillExtractorNotConfiguredException();
}
