namespace JobLens.Web.Models;

public class DashboardViewModel
{
    public string Greeting { get; init; } = "";
    public DateOnly Today { get; init; }

    public int WeeklyGoal { get; init; }
    public int AppliedThisWeek { get; init; }

    public int Total { get; init; }
    public int Active { get; init; }
    public int Interviews { get; init; }
    public int Offers { get; init; }

    public int? DaysSinceLastApplied { get; init; }
    public int WeekStreak { get; init; }

    public IReadOnlyList<JobApplication> UpNext { get; init; } = [];
    public IReadOnlyList<JobApplication> Recent { get; init; } = [];
    public IReadOnlyDictionary<ApplicationStatus, int> ByStatus { get; init; } =
        new Dictionary<ApplicationStatus, int>();

    public string Encouragement { get; init; } = "";
    public string Quote { get; init; } = "";
}
