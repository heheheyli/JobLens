using JobLens.Web.Services;

namespace JobLens.Web.Models;

public class AnalyserViewModel
{
    public string? AdText { get; set; }
    public int? ApplicationId { get; set; }
    public List<string> Extracted { get; set; } = [];

    public IReadOnlyList<SavedAd> SavedAds { get; set; } = [];
    public int ProfileSkillCount { get; set; }
    public SkillMatch? Result { get; set; }
    public string? Error { get; set; }
    public string? Added { get; set; }
}

public record SavedAd(int Id, string Label);
