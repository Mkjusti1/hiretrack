using HireTrack.Api.Common;
using HireTrack.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HireTrack.Api.Features.Applications;

[ApiController]
[Route("api/export")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TenantContext _tenant;

    public ExportController(AppDbContext db, TenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet("applications")]
    public async Task<IActionResult> ExportApplications([FromQuery] Guid? jobId)
    {
        var query = _db.Applications
            .Where(a => a.TenantId == _tenant.TenantId)
            .Include(a => a.Job)
            .Include(a => a.Candidate)
            .Include(a => a.Events)
            .AsQueryable();

        if (jobId.HasValue)
            query = query.Where(a => a.JobId == jobId.Value);

        var applications = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Candidate Name,Email,Phone,Job Title,Department,Stage,Applied At,Last Updated,Cover Note,Stage History");

        foreach (var app in applications)
        {
            var stageHistory = string.Join(" > ", app.Events
                .OrderBy(e => e.CreatedAt)
                .Select(e => e.ToStage.ToString()));

            var line = string.Join(",", new[]
            {
                Escape(app.Candidate?.Name),
                Escape(app.Candidate?.Email),
                Escape(app.Candidate?.Phone),
                Escape(app.Job?.Title),
                Escape(app.Job?.Department),
                Escape(app.Stage.ToString()),
                app.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                app.UpdatedAt.ToString("yyyy-MM-dd HH:mm"),
                Escape(app.CoverNote),
                Escape(stageHistory)
            });

            sb.AppendLine(line);
        }

        var fileName = jobId.HasValue
            ? $"applications-{jobId}-{DateTime.UtcNow:yyyyMMdd}.csv"
            : $"applications-all-{DateTime.UtcNow:yyyyMMdd}.csv";

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", fileName);
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
