using JobLens.Web.Models;
using JobLens.Web.Services;

namespace JobLens.Tests;

public class DashboardStatsTests
{
    private static readonly DateOnly Today = new(2026, 10, 15);

    private static JobApplication App(ApplicationStatus status, int daysAgo = 0) => new()
    {
        Employer = "Acme",
        Role = "Developer",
        Status = status,
        DateApplied = Today.AddDays(-daysAgo),
    };

    [Fact]
    public void CountByStatus_ListsEveryStatusEvenWhenTheCountIsZero()
    {
        var counts = DashboardStats.CountByStatus(new[]
        {
            App(ApplicationStatus.Applied),
            App(ApplicationStatus.Applied),
            App(ApplicationStatus.Interview),
        });

        Assert.Equal(Enum.GetValues<ApplicationStatus>().Length, counts.Count);
        Assert.Equal(2, counts[ApplicationStatus.Applied]);
        Assert.Equal(1, counts[ApplicationStatus.Interview]);
        Assert.Equal(0, counts[ApplicationStatus.Offer]);
    }

    [Fact]
    public void ResponseRate_IsNullWhenNothingHasBeenSent()
    {
        Assert.Null(DashboardStats.ResponseRate(new[] { App(ApplicationStatus.WantToApply) }));
    }

    [Fact]
    public void ResponseRate_CountsInterviewsOffersAndRejectionsAsReplies()
    {
        var apps = new[]
        {
            App(ApplicationStatus.Applied),
            App(ApplicationStatus.Interview),
            App(ApplicationStatus.Rejected),
            App(ApplicationStatus.Withdrawn),     // the user's own choice, not a reply
            App(ApplicationStatus.WantToApply),   // not sent, so not counted at all
        };

        Assert.Equal(50, DashboardStats.ResponseRate(apps));
    }

    [Fact]
    public void ResponseRate_RoundsToAWholePercentage()
    {
        var apps = new[]
        {
            App(ApplicationStatus.Offer),
            App(ApplicationStatus.Applied),
            App(ApplicationStatus.Applied),
        };

        Assert.Equal(33, DashboardStats.ResponseRate(apps));
    }

    [Fact]
    public void AppliedPastWeek_CountsTheLastSevenDaysIncludingToday()
    {
        var apps = new[]
        {
            App(ApplicationStatus.Applied, daysAgo: 0),
            App(ApplicationStatus.Applied, daysAgo: 6),
            App(ApplicationStatus.Applied, daysAgo: 7),            // too old
            App(ApplicationStatus.Applied, daysAgo: -1),           // in the future
            App(ApplicationStatus.WantToApply, daysAgo: 0),        // saved, not sent
        };

        Assert.Equal(2, DashboardStats.AppliedPastWeek(apps, Today));
    }

    [Fact]
    public void DayStreak_CountsConsecutiveDaysEndingToday()
    {
        var apps = new[]
        {
            App(ApplicationStatus.Applied, 0),
            App(ApplicationStatus.Applied, 1),
            App(ApplicationStatus.Applied, 2),
            App(ApplicationStatus.Applied, 4),   // gap on day 3 breaks the run
        };

        Assert.Equal(3, DashboardStats.DayStreak(apps, Today));
    }

    [Fact]
    public void DayStreak_IsKeptWhenTheLastApplicationWasYesterday()
    {
        var apps = new[] { App(ApplicationStatus.Applied, 1), App(ApplicationStatus.Applied, 2) };

        Assert.Equal(2, DashboardStats.DayStreak(apps, Today));
    }

    [Fact]
    public void DayStreak_BreaksAfterAWholeDayWithNoApplications()
    {
        var apps = new[] { App(ApplicationStatus.Applied, 2), App(ApplicationStatus.Applied, 3) };

        Assert.Equal(0, DashboardStats.DayStreak(apps, Today));
    }

    [Fact]
    public void DayStreak_IgnoresApplicationsThatWereOnlySaved()
    {
        Assert.Equal(0, DashboardStats.DayStreak(new[] { App(ApplicationStatus.WantToApply, 0) }, Today));
    }

    [Fact]
    public void Stale_ReturnsAppliedApplicationsOlderThanTwoWeeksOldestFirst()
    {
        var oldest = App(ApplicationStatus.Applied, 20);
        var borderline = App(ApplicationStatus.Applied, 14);
        var apps = new[]
        {
            borderline,
            App(ApplicationStatus.Applied, 13),      // not stale yet
            App(ApplicationStatus.Interview, 30),    // already heard back
            oldest,
        };

        var stale = DashboardStats.Stale(apps, Today);

        Assert.Equal(new[] { oldest, borderline }, stale.ToArray());
    }
}
