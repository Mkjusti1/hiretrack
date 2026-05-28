namespace HireTrack.Api.Features.Jobs;

public record CreateJobRequest(
    string Title,
    string Department,
    string Location,
    string? Description
);

public record UpdateJobRequest(
    string Title,
    string Department,
    string Location,
    string? Description,
    string Status
);

public record JobResponse(
    Guid Id,
    string Title,
    string Department,
    string Location,
    string? Description,
    string Status,
    int ApplicationCount,
    DateTime CreatedAt
);
