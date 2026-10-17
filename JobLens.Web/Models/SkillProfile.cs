namespace JobLens.Web.Models;

public class SkillProfile
{
    public int Id { get; set; }
    public string? Summary { get; set; }
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}

// Stored as integers: never reuse or renumber a value.
public enum SkillCategory
{
    Technical = 0,
    Personal = 1,
    Other = 2,
}

public class Skill
{
    public int Id { get; set; }
    public int SkillProfileId { get; set; }
    public SkillProfile? Profile { get; set; }
    public string Name { get; set; } = "";
    public SkillCategory Category { get; set; }
    public int? YearsExperience { get; set; }
}
