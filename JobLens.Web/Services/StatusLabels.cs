using JobLens.Web.Models;

namespace JobLens.Web.Services;

public static class StatusLabels
{
    public static IReadOnlyList<ApplicationStatus> Ordered { get; } =
    [
        ApplicationStatus.WantToApply,
        ApplicationStatus.Applied,
        ApplicationStatus.Interview,
        ApplicationStatus.Offer,
        ApplicationStatus.Accepted,
        ApplicationStatus.Rejected,
        ApplicationStatus.Withdrawn,
    ];

    public static string Label(this ApplicationStatus status) => status switch
    {
        ApplicationStatus.WantToApply => "Want to apply",
        _ => status.ToString(),
    };

    public static bool IsSubmitted(this ApplicationStatus status) => status != ApplicationStatus.WantToApply;
}
