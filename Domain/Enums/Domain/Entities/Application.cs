using HireTrack.Api.Domain.Enums;

namespace HireTrack.Api.Domain.Entities;

public class Application
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid JobId { get; set; }
    public Guid CandidateId { get; set; }
    public ApplicationStage Stage { get; set; } = ApplicationStage.Applied;
    public string? CoverNote { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Tenant Tenant { get; set; } = null!;
    public Job Job { get; set; } = null!;
    public Candidate Candidate { get; set; } = null!;
    public ICollection<ApplicationEvent> Events { get; set; } = [];
}