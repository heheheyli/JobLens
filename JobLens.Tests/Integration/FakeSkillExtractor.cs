using JobLens.Web.Services;

namespace JobLens.Tests.Integration;

/// <summary>Stands in for Azure AI Language so the tests never call the real service.</summary>
public class FakeSkillExtractor : ISkillExtractor
{
    public Task<IReadOnlyList<string>> ExtractAsync(string text, CancellationToken ct = default)
    {
        IReadOnlyList<string> skills = new[] { "C#", "Docker", "SQL" };
        return Task.FromResult(skills);
    }
}
