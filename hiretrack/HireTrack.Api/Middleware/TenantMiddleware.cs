using System.Security.Claims;
using HireTrack.Api.Common;

namespace HireTrack.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User.FindFirst("sub")?.Value;
            var roleClaim = context.User.FindFirst("role")?.Value ?? string.Empty;

            if (Guid.TryParse(tenantIdClaim, out var tenantId) &&
                Guid.TryParse(userIdClaim, out var userId))
            {
                tenantContext.Set(tenantId, userId, roleClaim);
            }
        }

        await _next(context);
    }
}
