using System.Text.RegularExpressions;

namespace JobLens.Web.Services;

public record SkillMatch(IReadOnlyList<string> Matched, IReadOnlyList<string> Missing, int? Score);

public static partial class SkillMatcher
{
    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["js"] = "javascript",
        ["ts"] = "typescript",
        ["reactjs"] = "react",
        ["react.js"] = "react",
        ["node"] = "node.js",
        ["nodejs"] = "node.js",
        ["dotnet"] = ".net",
        ["postgres"] = "postgresql",
        ["k8s"] = "kubernetes",
        ["csharp"] = "c#",
        ["c sharp"] = "c#",
        ["ms sql"] = "sql server",
        ["mssql"] = "sql server",
    };

    /// <summary>Lower-cases, tidies spacing and punctuation, and maps common aliases.</summary>
    public static string Normalise(string skill)
    {
        var s = Whitespace().Replace(skill.Trim().ToLowerInvariant(), " ");
        s = s.Trim('(', ')', '"', '\'').TrimEnd('.', ',', ';', ':');
        return Aliases.GetValueOrDefault(s, s);
    }

    /// <summary>
    /// Splits the ad's skills into ones the profile has and ones it doesn't.
    /// Score is the percentage of the ad's skills the profile covers, or null if the ad had none.
    /// </summary>
    public static SkillMatch Match(IEnumerable<string> adSkills, IEnumerable<string> profileSkills)
    {
        var have = profileSkills.Select(Normalise).ToHashSet();
        var seen = new HashSet<string>();
        var matched = new List<string>();
        var missing = new List<string>();

        foreach (var skill in adSkills)
        {
            var key = Normalise(skill);
            if (key.Length == 0 || !seen.Add(key))
            {
                continue;
            }

            (have.Contains(key) ? matched : missing).Add(skill.Trim());
        }

        var total = matched.Count + missing.Count;
        int? score = total == 0
            ? null
            : (int)Math.Round(matched.Count * 100.0 / total, MidpointRounding.AwayFromZero);

        return new SkillMatch(matched, missing, score);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
