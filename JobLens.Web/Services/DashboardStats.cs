using JobLens.Web.Models;

namespace JobLens.Web.Services;

public static class DashboardStats
{
    public const int PastWeekDays = 7;
    public const int StaleAfterDays = 14;

    // Withdrawn is the user's choice, not a reply from the employer.
    private static readonly ApplicationStatus[] HeardBack =
    [
        ApplicationStatus.Interview,
        ApplicationStatus.Offer,
        ApplicationStatus.Accepted,
        ApplicationStatus.Rejected,
    ];

    public static IReadOnlyDictionary<ApplicationStatus, int> CountByStatus(IEnumerable<JobApplication> apps)
    {
        var counts = StatusLabels.Ordered.ToDictionary(s => s, _ => 0);
        foreach (var a in apps)
        {
            counts[a.Status] = counts.GetValueOrDefault(a.Status) + 1;
        }
        return counts;
    }

    /// <summary>Percentage of sent applications that heard back, or null if none have been sent.</summary>
    public static int? ResponseRate(IEnumerable<JobApplication> apps)
    {
        var sent = apps.Where(a => a.Status.IsSubmitted()).ToList();
        if (sent.Count == 0)
        {
            return null;
        }

        var heard = sent.Count(a => HeardBack.Contains(a.Status));
        return (int)Math.Round(heard * 100.0 / sent.Count, MidpointRounding.AwayFromZero);
    }

    /// <summary>Applications sent in the last 7 days, today included.</summary>
    public static int AppliedPastWeek(IEnumerable<JobApplication> apps, DateOnly today)
    {
        var from = today.AddDays(-(PastWeekDays - 1));
        return apps.Count(a => a.Status.IsSubmitted() && a.DateApplied >= from && a.DateApplied <= today);
    }

    /// <summary>
    /// Consecutive days with at least one application sent. A streak that ended
    /// yesterday still counts, so it only breaks once a whole day passes with none.
    /// </summary>
    public static int DayStreak(IEnumerable<JobApplication> apps, DateOnly today)
    {
        var days = apps.Where(a => a.Status.IsSubmitted()).Select(a => a.DateApplied).ToHashSet();

        var cursor = days.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;
        while (days.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }
        return streak;
    }

    /// <summary>Still on Applied 14 or more days after sending, oldest first.</summary>
    public static IReadOnlyList<JobApplication> Stale(IEnumerable<JobApplication> apps, DateOnly today) =>
        apps.Where(a => a.Status == ApplicationStatus.Applied
                && today.DayNumber - a.DateApplied.DayNumber >= StaleAfterDays)
            .OrderBy(a => a.DateApplied)
            .ToList();
}
