using HireTrack.Api.Domain.Enums;

namespace HireTrack.Api.Domain.Entities;

public class ApplicationEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public Guid ActorId { get; set; }
    public ApplicationStage? FromStage { get; set; }
    public ApplicationStage ToStage { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Application Application { get; set; } = null!;
    public AppUser Actor { get; set; } = null!;
}
