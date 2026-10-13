namespace JobLens.Web.Models;

public class DashboardViewModel
{
    public string Greeting { get; init; } = "";
    public DateOnly Today { get; init; }

    public int WeeklyGoal { get; init; }
    public int AppliedPastWeek { get; init; }

    public int Total { get; init; }
    public int? ResponseRate { get; init; }
    public int Active { get; init; }
    public int Interviews { get; init; }
    public int Offers { get; init; }
    public int WantToApply { get; init; }

    public int? DaysSinceLastApplied { get; init; }
    public int DayStreak { get; init; }
    public bool SentToday { get; init; }

    public IReadOnlyList<JobApplication> UpNext { get; init; } = [];
    public IReadOnlyList<JobApplication> Recent { get; init; } = [];
    public IReadOnlyList<JobApplication> Stale { get; init; } = [];
    public IReadOnlyDictionary<ApplicationStatus, int> ByStatus { get; init; } =
        new Dictionary<ApplicationStatus, int>();

    public string Encouragement { get; init; } = "";
}
