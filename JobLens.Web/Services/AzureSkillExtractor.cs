using Azure.AI.TextAnalytics;

namespace JobLens.Web.Services;

public class AzureSkillExtractor : ISkillExtractor
{
    // The service rejects documents over 5,120 characters and NER batches over 5 documents.
    private const int MaxChunkLength = 5000;
    private const int MaxDocumentsPerRequest = 5;
    private const double MinConfidence = 0.6;

    private readonly TextAnalyticsClient _client;

    public AzureSkillExtractor(TextAnalyticsClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<string>> ExtractAsync(string text, CancellationToken ct = default)
    {
        var skills = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var batch in Chunk(text).Chunk(MaxDocumentsPerRequest))
        {
            var response = await _client.RecognizeEntitiesBatchAsync(batch, "en", cancellationToken: ct);

            foreach (var doc in response.Value)
            {
                if (doc.HasError)
                {
                    throw new InvalidOperationException(doc.Error.Message);
                }

                var source = batch[int.Parse(doc.Id)];
                foreach (var entity in doc.Entities)
                {
                    var name = WithSuffix(entity.Text, source, entity.Offset + entity.Length).Trim();
                    if (entity.Category == EntityCategory.Skill
                        && entity.ConfidenceScore >= MinConfidence
                        && seen.Add(name))
                    {
                        skills.Add(name);
                    }
                }
            }
        }

        return skills;
    }

    /// <summary>The service drops symbols from names like C# and C++, so put them back from the source text.</summary>
    public static string WithSuffix(string name, string source, int end)
    {
        foreach (var suffix in new[] { "++", "#" })
        {
            if (end >= 0 && end + suffix.Length <= source.Length
                && string.CompareOrdinal(source, end, suffix, 0, suffix.Length) == 0
                && !name.EndsWith(suffix, StringComparison.Ordinal))
            {
                return name + suffix;
            }
        }
        return name;
    }

    public static IEnumerable<string> Chunk(string text)
    {
        var rest = text.Trim();
        while (rest.Length > MaxChunkLength)
        {
            var cut = rest.LastIndexOfAny([' ', '\n', '\t'], MaxChunkLength);
            if (cut <= 0)
            {
                cut = MaxChunkLength;
            }

            yield return rest[..cut];
            rest = rest[cut..].TrimStart();
        }

        if (rest.Length > 0)
        {
            yield return rest;
        }
    }
}
