using HireTrack.Api.Common;
using HireTrack.Api.Data;
using HireTrack.Api.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireTrack.Api.Features.Interviews;

[ApiController]
[Route("api/interviews")]
[Authorize]
public class InterviewsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TenantContext _tenant;

    public InterviewsController(AppDbContext db, TenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? applicationId)
    {
        var query = _db.Interviews
            .Where(i => i.TenantId == _tenant.TenantId)
            .Include(i => i.Application)
                .ThenInclude(a => a.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a.Job)
            .Include(i => i.Interviewer)
            .AsQueryable();

        if (applicationId.HasValue)
            query = query.Where(i => i.ApplicationId == applicationId.Value);

        var interviews = await query
            .OrderBy(i => i.ScheduledAt)
            .ToListAsync();

return Ok(interviews.Select(i => MapToResponse(i)));    
}

    [HttpPost]
    public async Task<IActionResult> Schedule(ScheduleInterviewRequest req)
    {
        var application = await _db.Applications
            .Include(a => a.Candidate)
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.TenantId == _tenant.TenantId && a.Id == req.ApplicationId);

        if (application == null)
            return NotFound(new { message = "Application not found." });

        var interviewer = await _db.Users
            .FirstOrDefaultAsync(u => u.TenantId == _tenant.TenantId && u.Id == req.InterviewerId);

        if (interviewer == null)
            return NotFound(new { message = "Interviewer not found." });

        var interview = new Interview
        {
            ApplicationId = req.ApplicationId,
            InterviewerId = req.InterviewerId,
            TenantId = _tenant.TenantId,
            ScheduledAt = req.ScheduledAt,
            Location = req.Location,
            Notes = req.Notes
        };

        _db.Interviews.Add(interview);
        await _db.SaveChangesAsync();

        await _db.Entry(interview).Reference(i => i.Interviewer).LoadAsync();

        return CreatedAtAction(nameof(GetAll), new { applicationId = interview.ApplicationId }, MapToResponse(interview, application));
    }

    [HttpPut("{id}/feedback")]
    public async Task<IActionResult> SubmitFeedback(Guid id, SubmitFeedbackRequest req)
    {
        if (req.Rating < 1 || req.Rating > 5)
            return BadRequest(new { message = "Rating must be between 1 and 5." });

        var interview = await _db.Interviews
            .Include(i => i.Application)
                .ThenInclude(a => a.Candidate)
            .Include(i => i.Application)
                .ThenInclude(a => a.Job)
            .Include(i => i.Interviewer)
            .FirstOrDefaultAsync(i => i.TenantId == _tenant.TenantId && i.Id == id);

        if (interview == null) return NotFound();

        if (interview.FeedbackSubmitted)
            return Conflict(new { message = "Feedback already submitted for this interview." });

        interview.FeedbackSubmitted = true;
        interview.Rating = req.Rating;
        interview.FeedbackNotes = req.FeedbackNotes;

        await _db.SaveChangesAsync();

        return Ok(MapToResponse(interview));
    }

    private static InterviewResponse MapToResponse(Interview i, Application? app = null)
    {
        var application = app ?? i.Application;
        return new InterviewResponse(
            i.Id,
            i.ApplicationId,
            application?.Candidate?.Name ?? string.Empty,
            application?.Job?.Title ?? string.Empty,
            i.InterviewerId,
            i.Interviewer != null ? $"{i.Interviewer.FirstName} {i.Interviewer.LastName}" : string.Empty,
            i.ScheduledAt,
            i.Location,
            i.Notes,
            i.FeedbackSubmitted,
            i.Rating,
            i.FeedbackNotes,
            i.CreatedAt
        );
    }
}
