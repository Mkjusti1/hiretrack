namespace HireTrack.Api.Features.Interviews;

public record ScheduleInterviewRequest(
    Guid ApplicationId,
    Guid InterviewerId,
    DateTime ScheduledAt,
    string? Location,
    string? Notes
);

public record SubmitFeedbackRequest(
    int Rating,
    string FeedbackNotes
);

public record InterviewResponse(
    Guid Id,
    Guid ApplicationId,
    string CandidateName,
    string JobTitle,
    Guid InterviewerId,
    string InterviewerName,
    DateTime ScheduledAt,
    string? Location,
    string? Notes,
    bool FeedbackSubmitted,
    int? Rating,
    string? FeedbackNotes,
    DateTime CreatedAt
);
