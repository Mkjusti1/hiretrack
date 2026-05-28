namespace HireTrack.Api.Features.Auth;

public record RegisterRequest(
    string CompanyName,
    string FirstName,
    string LastName,
    string Email,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    Guid TenantId
);
