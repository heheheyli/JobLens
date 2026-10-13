using Microsoft.AspNetCore.Mvc;
using JobLens.Web.Data;
using JobLens.Web.Models;

namespace JobLens.Web.Controllers;

public class ProfileController : Controller
{
    private static readonly char[] Separators = [',', ';', '\n', '\r'];

    private readonly JobLensDbContext _context;

    public ProfileController(JobLensDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.GetOrCreateProfileAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string? skills, SkillCategory category)
    {
        if (!Enum.IsDefined(category))
        {
            return BadRequest();
        }

        var profile = await _context.GetOrCreateProfileAsync();
        var added = profile.AddSkills((skills ?? "").Split(Separators, StringSplitOptions.RemoveEmptyEntries), category);

        if (added > 0)
        {
            await _context.SaveChangesAsync();
            TempData["Flash"] = added == 1 ? "Skill added." : $"{added} skills added.";
        }
        else if (!string.IsNullOrWhiteSpace(skills))
        {
            TempData["Flash"] = "Already in your skills.";
        }
        TempData["FlashCategory"] = category.ToString();

        return RedirectToAction(nameof(Index), null, category.ToString().ToLowerInvariant());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill is null)
        {
            return RedirectToAction(nameof(Index));
        }

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), null, skill.Category.ToString().ToLowerInvariant());
    }
}
