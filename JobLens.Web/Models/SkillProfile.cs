namespace JobLens.Web.Models;

public class SkillProfile
{
    public int Id { get; set; }
    public string? Summary { get; set; }
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}

public class Skill
{
    public int Id { get; set; }
    public int SkillProfileId { get; set; }
    public SkillProfile? Profile { get; set; }
    public string Name { get; set; } = "";
    public int? YearsExperience { get; set; }
}