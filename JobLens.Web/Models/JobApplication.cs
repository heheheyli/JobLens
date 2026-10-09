using System.ComponentModel.DataAnnotations;

namespace JobLens.Web.Models;

public class JobApplication
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Who's it with? Add the employer.")]
    [MaxLength(120)]
    public string Employer { get; set; } = "";

    [Required(ErrorMessage = "Add the job title.")]
    [MaxLength(160)]
    [Display(Name = "Job title")]
    public string Role { get; set; } = "";

    [MaxLength(120)]
    public string? Location { get; set; }

    [MaxLength(60)]
    [Display(Name = "Found via")]
    public string? Source { get; set; }          // LinkedIn, Seek, referral…

    [MaxLength(500)]
    [Url(ErrorMessage = "That doesn't look like a link. Try pasting the full address.")]
    [Display(Name = "Listing link")]
    public string? Url { get; set; }

    [Display(Name = "Date applied")]
    public DateOnly DateApplied { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    [Display(Name = "Next action")]
    public DateOnly? NextActionDate { get; set; }

    public string? Notes { get; set; }

    [Display(Name = "Job ad")]
    public string? AdvertisementText { get; set; }   // the Analyser reads this

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StatusChange> StatusHistory { get; set; } = new List<StatusChange>();
}
