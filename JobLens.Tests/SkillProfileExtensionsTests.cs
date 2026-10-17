using JobLens.Web.Data;
using JobLens.Web.Models;

namespace JobLens.Tests;

public class SkillProfileExtensionsTests
{
    [Fact]
    public void AddSkills_AddsNewNamesAndReportsHowManyWereAdded()
    {
        var profile = new SkillProfile();

        var added = profile.AddSkills(new[] { "C#", "Docker" });

        Assert.Equal(2, added);
        Assert.Equal(new[] { "C#", "Docker" }, profile.Skills.Select(s => s.Name).ToArray());
    }

    [Fact]
    public void AddSkills_SkipsSkillsTheProfileAlreadyHasIncludingAliases()
    {
        var profile = new SkillProfile();
        profile.Skills.Add(new Skill { Name = "node.js" });

        var added = profile.AddSkills(new[] { "NodeJS", "node" });

        Assert.Equal(0, added);
        Assert.Single(profile.Skills);
    }

    [Fact]
    public void AddSkills_IgnoresDuplicatesWithinTheSameCall()
    {
        var profile = new SkillProfile();

        Assert.Equal(1, profile.AddSkills(new[] { "SQL", "sql" }));
    }

    [Fact]
    public void AddSkills_TrimsNamesAndIgnoresBlankOrOversizedOnes()
    {
        var profile = new SkillProfile();

        var added = profile.AddSkills(new[] { "  Go  ", "", "   ", new string('x', 101) });

        Assert.Equal(1, added);
        Assert.Equal("Go", profile.Skills.Single().Name);
    }

    [Fact]
    public void AddSkills_UsesTheGivenCategory()
    {
        var profile = new SkillProfile();

        profile.AddSkills(new[] { "Teamwork" }, SkillCategory.Personal);

        Assert.Equal(SkillCategory.Personal, profile.Skills.Single().Category);
    }
}
