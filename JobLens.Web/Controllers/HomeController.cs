using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobLens.Web.Data;
using JobLens.Web.Models;

namespace JobLens.Web.Controllers;

public class HomeController : Controller
{
    private const int WeeklyGoal = 5;

    private static readonly ApplicationStatus[] ActiveStatuses =
        [ApplicationStatus.Applied, ApplicationStatus.Screening, ApplicationStatus.Interview, ApplicationStatus.Offer];

    private static readonly string[] Quotes =
    [
        "Every application is a door you opened yourself.",
        "You only need one yes.",
        "Small steps, sent often, add up to big changes.",
        "Rejection is redirection. Keep going.",
        "The right team is out there looking for someone like you.",
        "Be proud of the trying, not just the outcome.",
        "Today's effort is next month's offer.",
        "Soft heart, steady hands, one more application.",
    ];

    private readonly JobLensDbContext _context;

    public HomeController(JobLensDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

        var apps = await _context.JobApplications.AsNoTracking().ToListAsync();

        var lastApplied = apps.Count == 0 ? (DateOnly?)null : apps.Max(a => a.DateApplied);
        int? daysSince = lastApplied is { } d ? today.DayNumber - d.DayNumber : null;
        var appliedThisWeek = apps.Count(a => a.DateApplied >= weekStart && a.DateApplied <= today);

        var model = new DashboardViewModel
        {
            Greeting = now.Hour switch
            {
                < 5 => "Still up?",
                < 12 => "Good morning",
                < 19 => "Good afternoon",
                _ => "Good evening",
            },
            Today = today,
            WeeklyGoal = WeeklyGoal,
            AppliedThisWeek = appliedThisWeek,
            Total = apps.Count,
            Active = apps.Count(a => ActiveStatuses.Contains(a.Status)),
            Interviews = apps.Count(a => a.Status == ApplicationStatus.Interview),
            Offers = apps.Count(a => a.Status is ApplicationStatus.Offer or ApplicationStatus.Accepted),
            DaysSinceLastApplied = daysSince,
            WeekStreak = CountWeekStreak(apps, weekStart),
            UpNext = apps
                .Where(a => a.NextActionDate != null && ActiveStatuses.Contains(a.Status))
                .OrderBy(a => a.NextActionDate)
                .Take(5)
                .ToList(),
            Recent = apps
                .OrderByDescending(a => a.DateApplied)
                .ThenByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList(),
            ByStatus = apps.GroupBy(a => a.Status).ToDictionary(g => g.Key, g => g.Count()),
            Encouragement = Encourage(appliedThisWeek, daysSince, apps.Count),
            Quote = Quotes[today.DayOfYear % Quotes.Length],
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static int CountWeekStreak(List<JobApplication> apps, DateOnly weekStart)
    {
        var weeks = apps
            .Select(a => a.DateApplied.AddDays(-(((int)a.DateApplied.DayOfWeek + 6) % 7)))
            .ToHashSet();

        var cursor = weeks.Contains(weekStart) ? weekStart : weekStart.AddDays(-7);
        var streak = 0;
        while (weeks.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-7);
        }
        return streak;
    }

    private static string Encourage(int thisWeek, int? daysSince, int total) => (thisWeek, daysSince, total) switch
    {
        (_, _, 0) => "Your first application is waiting. Let's make it a good one.",
        ( >= WeeklyGoal, _, _) => "Weekly goal reached. Look at you go!",
        (_, 0, _) => "You sent one today. That counts for a lot.",
        (_, <= 2, _) => "You're in a lovely rhythm. One more?",
        (_, <= 6, _) => "A few quiet days. A small one today keeps the momentum.",
        _ => "It's been a little while. No pressure, just one to warm back up.",
    };
}
