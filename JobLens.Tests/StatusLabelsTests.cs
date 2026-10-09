using JobLens.Web.Models;
using JobLens.Web.Services;

namespace JobLens.Tests;

public class StatusLabelsTests
{
    [Fact]
    public void Ordered_ListsEveryStatusOnce_StartingWithWantToApply()
    {
        Assert.Equal(ApplicationStatus.WantToApply, StatusLabels.Ordered[0]);
        Assert.Equal(
            Enum.GetValues<ApplicationStatus>().OrderBy(s => s),
            StatusLabels.Ordered.OrderBy(s => s));
    }

    [Theory]
    [InlineData(ApplicationStatus.WantToApply, "Want to apply")]
    [InlineData(ApplicationStatus.Applied, "Applied")]
    [InlineData(ApplicationStatus.Interview, "Interview")]
    [InlineData(ApplicationStatus.Withdrawn, "Withdrawn")]
    public void Label_IsReadable(ApplicationStatus status, string expected)
    {
        Assert.Equal(expected, status.Label());
    }

    [Theory]
    [InlineData(ApplicationStatus.WantToApply, false)]
    [InlineData(ApplicationStatus.Applied, true)]
    [InlineData(ApplicationStatus.Interview, true)]
    [InlineData(ApplicationStatus.Offer, true)]
    [InlineData(ApplicationStatus.Accepted, true)]
    [InlineData(ApplicationStatus.Rejected, true)]
    [InlineData(ApplicationStatus.Withdrawn, true)]
    public void IsSubmitted_IsFalseOnlyForWantToApply(ApplicationStatus status, bool expected)
    {
        Assert.Equal(expected, status.IsSubmitted());
    }

    [Fact]
    public void StoredValues_DoNotChange()
    {
        Assert.Equal(0, (int)ApplicationStatus.Applied);
        Assert.Equal(2, (int)ApplicationStatus.Interview);
        Assert.Equal(3, (int)ApplicationStatus.Offer);
        Assert.Equal(4, (int)ApplicationStatus.Accepted);
        Assert.Equal(5, (int)ApplicationStatus.Rejected);
        Assert.Equal(6, (int)ApplicationStatus.Withdrawn);
        Assert.Equal(7, (int)ApplicationStatus.WantToApply);
        Assert.False(Enum.IsDefined(typeof(ApplicationStatus), 1));
    }
}
