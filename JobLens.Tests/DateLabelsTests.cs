using JobLens.Web.Services;

namespace JobLens.Tests;

public class DateLabelsTests
{
    private static readonly DateOnly Today = new(2026, 10, 9);

    [Theory]
    [InlineData(-3, "upcoming")]
    [InlineData(0, "today")]
    [InlineData(1, "yesterday")]
    [InlineData(2, "2 days ago")]
    [InlineData(6, "6 days ago")]
    [InlineData(7, "last week")]
    [InlineData(13, "last week")]
    [InlineData(14, "2 weeks ago")]
    [InlineData(59, "8 weeks ago")]
    [InlineData(60, "2 months ago")]
    [InlineData(365, "12 months ago")]
    public void Ago_DescribesHowLongSinceTheDate(int daysBefore, string expected)
    {
        var date = Today.AddDays(-daysBefore);

        Assert.Equal(expected, DateLabels.Ago(date, Today));
    }

    [Fact]
    public void Due_WithNoDate_IsNone()
    {
        var due = DateLabels.Due(null, Today);

        Assert.Equal(DueState.None, due.State);
        Assert.Equal("", due.Text);
        Assert.Equal("", due.CssClass);
    }

    [Theory]
    [InlineData(-5, "5d overdue", DueState.Overdue, "due-late")]
    [InlineData(-1, "1d overdue", DueState.Overdue, "due-late")]
    [InlineData(0, "Today", DueState.Today, "due-today")]
    [InlineData(1, "Tomorrow", DueState.Soon, "due-soon")]
    [InlineData(2, "In 2 days", DueState.Soon, "due-soon")]
    [InlineData(6, "In 6 days", DueState.Soon, "due-soon")]
    [InlineData(7, "16 Oct", DueState.Later, "")]
    [InlineData(30, "8 Nov", DueState.Later, "")]
    public void Due_LabelsTheFollowUpRelativeToToday(int daysAhead, string text, DueState state, string css)
    {
        var due = DateLabels.Due(Today.AddDays(daysAhead), Today);

        Assert.Equal(text, due.Text);
        Assert.Equal(state, due.State);
        Assert.Equal(css, due.CssClass);
    }
}
