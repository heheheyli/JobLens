using JobLens.Web.Services;

namespace JobLens.Tests;

public class SkillMatcherTests
{
    [Theory]
    [InlineData("C#", "c#")]
    [InlineData("  Docker  ", "docker")]
    [InlineData("JS", "javascript")]
    [InlineData("ReactJS", "react")]
    [InlineData("Node", "node.js")]
    [InlineData("Postgres", "postgresql")]
    [InlineData("C Sharp", "c#")]
    [InlineData("SQL,", "sql")]
    [InlineData("(Kubernetes)", "kubernetes")]
    [InlineData("Node.js.", "node.js")]
    public void Normalise_LowerCasesTidiesAndMapsAliases(string input, string expected)
    {
        Assert.Equal(expected, SkillMatcher.Normalise(input));
    }

    [Fact]
    public void Match_SplitsAdSkillsIntoMatchedAndMissing()
    {
        var result = SkillMatcher.Match(new[] { "C#", "Docker", "SQL" }, new[] { "c#", "sql" });

        Assert.Equal(new[] { "C#", "SQL" }, result.Matched.ToArray());
        Assert.Equal(new[] { "Docker" }, result.Missing.ToArray());
        Assert.Equal(67, result.Score);
    }

    [Fact]
    public void Match_TreatsAliasesAsTheSameSkill()
    {
        var result = SkillMatcher.Match(new[] { "PostgreSQL" }, new[] { "Postgres" });

        Assert.Single(result.Matched);
        Assert.Empty(result.Missing);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Match_CountsRepeatedSkillsOnlyOnce()
    {
        var result = SkillMatcher.Match(new[] { "Node", "NodeJS", "node.js" }, new[] { "node.js" });

        Assert.Single(result.Matched);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Match_IgnoresBlankSkillsInTheAd()
    {
        var result = SkillMatcher.Match(new[] { "", "   ", "SQL" }, new[] { "SQL" });

        Assert.Single(result.Matched);
        Assert.Empty(result.Missing);
        Assert.Equal(100, result.Score);
    }

    [Fact]
    public void Match_WithEmptyProfile_ScoresZero()
    {
        var result = SkillMatcher.Match(new[] { "C#", "SQL" }, Array.Empty<string>());

        Assert.Empty(result.Matched);
        Assert.Equal(2, result.Missing.Count);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Match_WithNoSkillsInTheAd_HasNoScore()
    {
        var result = SkillMatcher.Match(Array.Empty<string>(), new[] { "C#" });

        Assert.Null(result.Score);
    }

    [Theory]
    [InlineData(1, 3, 33)]
    [InlineData(2, 3, 67)]
    [InlineData(1, 2, 50)]
    public void Match_RoundsTheScoreToAWholePercentage(int matched, int total, int expectedScore)
    {
        var adSkills = Enumerable.Range(1, total).Select(i => $"skill{i}").ToList();
        var profile = adSkills.Take(matched).ToList();

        Assert.Equal(expectedScore, SkillMatcher.Match(adSkills, profile).Score);
    }
}
