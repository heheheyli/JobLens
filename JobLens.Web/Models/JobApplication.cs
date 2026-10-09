using System.ComponentModel.DataAnnotations;

namespace JobLens.Web.Models;

public class JobApplication
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Employer { get; set; } = "";

    [Required, MaxLength(160)]
    public string Role { get; set; } = "";

    [MaxLength(120)]
    public string? Location { get; set; }

    [MaxLength(60)]
    public string? Source { get; set; }          // LinkedIn, Seek, referral…

    [MaxLength(500)]
    public string? Url { get; set; }

    public DateOnly DateApplied { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    public DateOnly? NextActionDate { get; set; }

    public string? Notes { get; set; }

    public string? AdvertisementText { get; set; }   // the Analyser reads this

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StatusChange> StatusHistory { get; set; } = new List<StatusChange>();
}