using JobLens.Web.Models;
using JobLens.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace JobLens.Web.Data;

public static class SkillProfileExtensions
{
    /// <summary>JobLens has a single user, so there's only ever one profile.</summary>
    public static async Task<SkillProfile> GetOrCreateProfileAsync(this JobLensDbContext db)
    {
        var profile = await db.SkillProfiles
            .Include(p => p.Skills)
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync();

        if (profile is null)
        {
            profile = new SkillProfile();
            db.SkillProfiles.Add(profile);
            await db.SaveChangesAsync();
        }

        return profile;
    }

    /// <summary>Adds each name the profile doesn't already have. Returns how many were added.</summary>
    public static int AddSkills(this SkillProfile profile, IEnumerable<string> names, SkillCategory category = SkillCategory.Technical)
    {
        var have = profile.Skills.Select(s => SkillMatcher.Normalise(s.Name)).ToHashSet();
        var added = 0;

        foreach (var raw in names)
        {
            var name = raw.Trim();
            if (name.Length is 0 or > 100 || !have.Add(SkillMatcher.Normalise(name)))
            {
                continue;
            }

            profile.Skills.Add(new Skill { Name = name, Category = category });
            added++;
        }

        return added;
    }
}
