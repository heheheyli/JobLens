namespace JobLens.Web.Services;

/// <summary>Pulls skill names out of a job ad. Swap in a fake for automated tests.</summary>
public interface ISkillExtractor
{
    Task<IReadOnlyList<string>> ExtractAsync(string text, CancellationToken ct = default);
}
