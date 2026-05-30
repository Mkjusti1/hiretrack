using HireTrack.Api.Common;
using HireTrack.Api.Data;
using HireTrack.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireTrack.Api.Features.Analytics;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TenantContext _tenant;

    public AnalyticsController(AppDbContext db, TenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipelineStats([FromQuery] Guid? jobId)
    {
        var query = _db.Applications
            .Where(a => a.TenantId == _tenant.TenantId)
            .Include(a => a.Events)
            .AsQueryable();

        if (jobId.HasValue)
            query = query.Where(a => a.JobId == jobId.Value);

        var applications = await query.ToListAsync();

        var total = applications.Count;
        var hired = applications.Count(a => a.Stage == ApplicationStage.Hired);
        var rejected = applications.Count(a => a.Stage == ApplicationStage.Rejected);
        var active = applications.Count(a => a.Stage != ApplicationStage.Hired && a.Stage != ApplicationStage.Rejected);
        var hireRate = total > 0 ? Math.Round((double)hired / total * 100, 1) : 0;

        // Applications by stage
        var byStage = Enum.GetValues<ApplicationStage>()
            .Select(stage => new
            {
                stage = stage.ToString(),
                count = applications.Count(a => a.Stage == stage)
            });

        // Average time in each stage (days)
        var stageTimings = new List<object>();
        foreach (var stage in Enum.GetValues<ApplicationStage>())
        {
            var times = new List<double>();
            foreach (var app in applications)
            {
                var entryEvent = app.Events
                    .Where(e => e.ToStage == stage)
                    .OrderBy(e => e.CreatedAt)
                    .FirstOrDefault();

                var exitEvent = app.Events
                    .Where(e => e.FromStage == stage)
                    .OrderBy(e => e.CreatedAt)
                    .FirstOrDefault();

                if (entryEvent != null && exitEvent != null)
                {
                    var days = (exitEvent.CreatedAt - entryEvent.CreatedAt).TotalDays;
                    times.Add(days);
                }
            }

            stageTimings.Add(new
            {
                stage = stage.ToString(),
                avgDays = times.Count > 0 ? Math.Round(times.Average(), 1) : 0,
                sampleSize = times.Count
            });
        }

        // Applications per day (last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var applicationsOverTime = applications
            .Where(a => a.CreatedAt >= thirtyDaysAgo)
            .GroupBy(a => a.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                date = g.Key.ToString("MMM dd"),
                count = g.Count()
            });

        return Ok(new
        {
            summary = new { total, hired, rejected, active, hireRate },
            byStage,
            stageTimings,
            applicationsOverTime
        });
    }
}
