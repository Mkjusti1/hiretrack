namespace HireTrack.Api.Domain.Entities;

public class Interview
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public Guid InterviewerId { get; set; }
    public Guid TenantId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public bool FeedbackSubmitted { get; set; } = false;
    public int? Rating { get; set; }
    public string? FeedbackNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;
    public AppUser Interviewer { get; set; } = null!;
}
