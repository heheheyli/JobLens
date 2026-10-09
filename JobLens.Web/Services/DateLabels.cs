using System.Globalization;

namespace JobLens.Web.Services;

public enum DueState
{
    None,
    Later,
    Soon,
    Today,
    Overdue,
}

public record DueLabel(string Text, DueState State)
{
    public string CssClass => State switch
    {
        DueState.Overdue => "due-late",
        DueState.Today => "due-today",
        DueState.Soon => "due-soon",
        _ => "",
    };
}

public static class DateLabels
{
    public static string Ago(DateOnly date, DateOnly today) => (today.DayNumber - date.DayNumber) switch
    {
        < 0 => "upcoming",
        0 => "today",
        1 => "yesterday",
        < 7 and var d => $"{d} days ago",
        < 14 => "last week",
        < 60 and var d => $"{d / 7} weeks ago",
        var d => $"{d / 30} months ago",
    };

    public static DueLabel Due(DateOnly? date, DateOnly today)
    {
        if (date is not { } d)
        {
            return new DueLabel("", DueState.None);
        }

        var days = d.DayNumber - today.DayNumber;
        return days switch
        {
            < 0 => new DueLabel($"{-days}d overdue", DueState.Overdue),
            0 => new DueLabel("Today", DueState.Today),
            1 => new DueLabel("Tomorrow", DueState.Soon),
            < 7 => new DueLabel($"In {days} days", DueState.Soon),
            _ => new DueLabel(d.ToString("d MMM", CultureInfo.InvariantCulture), DueState.Later),
        };
    }
}
