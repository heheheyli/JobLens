using System.ComponentModel.DataAnnotations;
using JobLens.Web.Models;

namespace JobLens.Tests;

public class JobApplicationValidationTests
{
    private static List<ValidationResult> Validate(JobApplication app)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(app, new ValidationContext(app), results, validateAllProperties: true);
        return results;
    }

    private static bool HasErrorOn(List<ValidationResult> results, string property) =>
        results.Any(r => r.MemberNames.Contains(property));

    [Fact]
    public void AnApplicationWithAnEmployerAndRoleIsValid()
    {
        var app = new JobApplication { Employer = "Acme", Role = "Developer" };

        Assert.Empty(Validate(app));
    }

    [Fact]
    public void TheEmployerIsRequired()
    {
        var app = new JobApplication { Employer = "", Role = "Developer" };

        Assert.True(HasErrorOn(Validate(app), nameof(JobApplication.Employer)));
    }

    [Fact]
    public void TheRoleIsRequired()
    {
        var app = new JobApplication { Employer = "Acme", Role = "" };

        Assert.True(HasErrorOn(Validate(app), nameof(JobApplication.Role)));
    }

    [Fact]
    public void TheEmployerCannotBeLongerThan120Characters()
    {
        var app = new JobApplication { Employer = new string('a', 121), Role = "Developer" };

        Assert.True(HasErrorOn(Validate(app), nameof(JobApplication.Employer)));
    }

    [Theory]
    [InlineData("not a link", true)]
    [InlineData("https://example.com/jobs/42", false)]
    [InlineData(null, false)]
    public void TheListingLinkMustLookLikeALink(string? url, bool shouldFail)
    {
        var app = new JobApplication { Employer = "Acme", Role = "Developer", Url = url };

        Assert.Equal(shouldFail, HasErrorOn(Validate(app), nameof(JobApplication.Url)));
    }
}
