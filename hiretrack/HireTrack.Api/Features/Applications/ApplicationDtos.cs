using HireTrack.Api.Domain.Enums;

namespace HireTrack.Api.Features.Applications;

public record CreateApplicationRequest(
    Guid JobId,
    string CandidateName,
    string CandidateEmail,
    string? CandidatePhone,
    string? CoverNote
);

public record MoveStageRequest(
    string ToStage,
    string? Note
);

public record ApplicationResponse(
    Guid Id,
    Guid JobId,
    string JobTitle,
    Guid CandidateId,
    string CandidateName,
    string CandidateEmail,
    string Stage,
    string? CoverNote,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<StageEventResponse> Events
);

public record StageEventResponse(
    string? FromStage,
    string ToStage,
    string? Note,
    DateTime CreatedAt
);
