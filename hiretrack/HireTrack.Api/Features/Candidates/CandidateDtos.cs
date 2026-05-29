namespace HireTrack.Api.Features.Candidates;

public record CandidateResponse(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? ResumeUrl,
    int ApplicationCount,
    DateTime CreatedAt
);
