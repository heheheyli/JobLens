using System.Net;
using System.Text.RegularExpressions;

namespace JobLens.Tests.Integration;

public static partial class FormHelper
{
    /// <summary>Loads a page and returns its anti-forgery token, which every POST needs.</summary>
    public static async Task<string> GetTokenAsync(HttpClient client, string url)
    {
        var html = await client.GetStringAsync(url);
        var match = Token().Match(html);
        Assert.True(match.Success, $"No anti-forgery token found on {url}");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    public static FormUrlEncodedContent Form(string token, params (string Name, string Value)[] fields)
    {
        var data = new Dictionary<string, string> { ["__RequestVerificationToken"] = token };
        foreach (var (name, value) in fields)
        {
            data[name] = value;
        }

        return new FormUrlEncodedContent(data);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex Token();
}
