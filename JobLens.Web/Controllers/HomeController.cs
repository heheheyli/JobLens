using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobLens.Web.Data;
using JobLens.Web.Models;
using JobLens.Web.Services;

namespace JobLens.Web.Controllers;

public class HomeController : Controller
{
    private const int WeeklyGoal = 5;

    private static readonly ApplicationStatus[] ActiveStatuses =
        [ApplicationStatus.Applied, ApplicationStatus.Interview, ApplicationStatus.Offer];

    private readonly JobLensDbContext _context;

    public HomeController(JobLensDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        var all = await _context.JobApplications.AsNoTracking().ToListAsync();
        var apps = all.Where(a => a.Status.IsSubmitted()).ToList();
        var wantToApply = all.Count - apps.Count;

        var lastApplied = apps.Count == 0 ? (DateOnly?)null : apps.Max(a => a.DateApplied);
        int? daysSince = lastApplied is { } d ? today.DayNumber - d.DayNumber : null;
        var appliedPastWeek = DashboardStats.AppliedPastWeek(all, today);

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
            AppliedPastWeek = appliedPastWeek,
            Total = apps.Count,
            ResponseRate = DashboardStats.ResponseRate(all),
            Active = apps.Count(a => ActiveStatuses.Contains(a.Status)),
            Interviews = apps.Count(a => a.Status == ApplicationStatus.Interview),
            Offers = apps.Count(a => a.Status is ApplicationStatus.Offer or ApplicationStatus.Accepted),
            DaysSinceLastApplied = daysSince,
            DayStreak = DashboardStats.DayStreak(all, today),
            SentToday = apps.Any(a => a.DateApplied == today),
            WantToApply = wantToApply,
            UpNext = all
                .Where(a => a.NextActionDate != null
                    && (ActiveStatuses.Contains(a.Status) || a.Status == ApplicationStatus.WantToApply))
                .OrderBy(a => a.NextActionDate)
                .Take(5)
                .ToList(),
            Recent = apps
                .OrderByDescending(a => a.DateApplied)
                .ThenByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList(),
            Stale = DashboardStats.Stale(all, today),
            ByStatus = DashboardStats.CountByStatus(all),
            Encouragement = Encourage(appliedPastWeek, daysSince, apps.Count, wantToApply),
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

    private static string Encourage(int pastWeek, int? daysSince, int total, int wantToApply) => (pastWeek, daysSince, total, wantToApply) switch
    {
        (_, _, 0, > 0) => $"{Saved(wantToApply)} and waiting. Pick one and send it!",
        (_, _, 0, _) => "Your first application is waiting. Let's make it a good one.",
        ( >= WeeklyGoal, _, _, _) => "Weekly goal reached. Look at you go!",
        (_, 0, _, _) => "You sent one today. That counts for a lot.",
        (_, <= 2, _, _) => "You're in a lovely rhythm. One more?",
        (_, _, _, > 0) => $"{Saved(wantToApply)} to apply for. Today's a good day for one.",
        (_, <= 6, _, _) => "A few quiet days. A small one today keeps the momentum.",
        _ => "It's been a little while. No pressure, just one to warm back up.",
    };

    private static string Saved(int n) => n == 1 ? "You've got 1 role saved" : $"You've got {n} roles saved";
}
