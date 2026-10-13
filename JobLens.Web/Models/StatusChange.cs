namespace JobLens.Web.Models;

public class StatusChange
{
    public int Id { get; set; }
    public int JobApplicationId { get; set; }
    public JobApplication? JobApplication { get; set; }

    public ApplicationStatus From { get; set; }
    public ApplicationStatus To { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
}