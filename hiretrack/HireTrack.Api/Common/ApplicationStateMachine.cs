using HireTrack.Api.Domain.Enums;

namespace HireTrack.Api.Common;

public static class ApplicationStateMachine
{
    private static readonly Dictionary<ApplicationStage, ApplicationStage[]> _allowedTransitions = new()
    {
        { ApplicationStage.Applied,   [ApplicationStage.Screened, ApplicationStage.Rejected] },
        { ApplicationStage.Screened,  [ApplicationStage.Interview, ApplicationStage.Rejected] },
        { ApplicationStage.Interview, [ApplicationStage.Offer, ApplicationStage.Rejected] },
        { ApplicationStage.Offer,     [ApplicationStage.Hired, ApplicationStage.Rejected] },
        { ApplicationStage.Hired,     [] },
        { ApplicationStage.Rejected,  [] }
    };

    public static bool CanTransition(ApplicationStage from, ApplicationStage to)
    {
        return _allowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static ApplicationStage[] GetAllowedTransitions(ApplicationStage from)
    {
        return _allowedTransitions.TryGetValue(from, out var allowed) ? allowed : [];
    }
}
