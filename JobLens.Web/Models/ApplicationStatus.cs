namespace JobLens.Web.Models;

// Stored as integers: never reuse or renumber a value. 1 was Screening.
public enum ApplicationStatus
{
    Applied = 0,
    Interview = 2,
    Offer = 3,
    Accepted = 4,
    Rejected = 5,
    Withdrawn = 6,
    WantToApply = 7,
}
