using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobLens.Web.Data;
using JobLens.Web.Models;
using JobLens.Web.Services;

namespace JobLens.Web.Controllers;

public class AnalyserController : Controller
{
    private const int MaxAdLength = 25000;

    private readonly JobLensDbContext _context;
    private readonly ISkillExtractor _extractor;
    private readonly ILogger<AnalyserController> _logger;

    public AnalyserController(JobLensDbContext context, ISkillExtractor extractor, ILogger<AnalyserController> logger)
    {
        _context = context;
        _extractor = extractor;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? applicationId)
    {
        var model = new AnalyserViewModel { ApplicationId = applicationId };

        if (applicationId is { } id)
        {
            model.AdText = await _context.JobApplications
                .Where(a => a.Id == id)
                .Select(a => a.AdvertisementText)
                .FirstOrDefaultAsync();
        }

        var profile = await _context.GetOrCreateProfileAsync();
        return View(await PopulateAsync(model, profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        [Bind("AdText,ApplicationId,Extracted")] AnalyserViewModel model, string? addSkill, CancellationToken ct)
    {
        var profile = await _context.GetOrCreateProfileAsync();

        if (!string.IsNullOrWhiteSpace(addSkill) && profile.AddSkills([addSkill]) > 0)
        {
            await _context.SaveChangesAsync(ct);
            model.Added = addSkill.Trim();
        }

        if (string.IsNullOrWhiteSpace(model.AdText))
        {
            model.Error = "Paste a job ad first, then I'll pick out the skills.";
            return View(await PopulateAsync(model, profile));
        }

        if (model.AdText.Length > MaxAdLength)
        {
            model.Error = $"That ad is a bit long. Keep it under {MaxAdLength:N0} characters.";
            return View(await PopulateAsync(model, profile));
        }

        // Adding a gap skill re-posts the skills already found, so the ad isn't sent to Azure twice.
        if (addSkill is null || model.Extracted.Count == 0)
        {
            try
            {
                model.Extracted = [.. await _extractor.ExtractAsync(model.AdText, ct)];
            }
            catch (SkillExtractorNotConfiguredException ex)
            {
                model.Error = ex.Message;
                return View(await PopulateAsync(model, profile));
            }
            catch (Exception ex) when (ex is RequestFailedException or InvalidOperationException or HttpRequestException)
            {
                _logger.LogError(ex, "Skill extraction failed");
                model.Error = "Couldn't reach the skill analyser just now. Try again in a moment.";
                return View(await PopulateAsync(model, profile));
            }
        }

        model.Result = SkillMatcher.Match(model.Extracted, profile.Skills.Select(s => s.Name));
        return View(await PopulateAsync(model, profile));
    }

    private async Task<AnalyserViewModel> PopulateAsync(AnalyserViewModel model, SkillProfile profile)
    {
        model.ProfileSkillCount = profile.Skills.Count;
        model.SavedAds = await _context.JobApplications
            .AsNoTracking()
            .Where(a => a.AdvertisementText != null && a.AdvertisementText != "")
            .OrderByDescending(a => a.DateApplied)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new SavedAd(a.Id, a.Role + " · " + a.Employer))
            .ToListAsync();
        return model;
    }
}
