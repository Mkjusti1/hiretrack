using HireTrack.Api.Common;
using HireTrack.Api.Data;
using HireTrack.Api.Domain.Entities;
using HireTrack.Api.Domain.Enums;
using HireTrack.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HireTrack.Api.Features.Applications;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TenantContext _tenant;
    private readonly IHubContext<PipelineHub> _hub;

    public ApplicationsController(AppDbContext db, TenantContext tenant, IHubContext<PipelineHub> hub)
    {
        _db = db;
        _tenant = tenant;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? jobId)
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

        return Ok(applications.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var application = await _db.Applications
            .Where(a => a.TenantId == _tenant.TenantId && a.Id == id)
            .Include(a => a.Job)
            .Include(a => a.Candidate)
            .Include(a => a.Events)
            .FirstOrDefaultAsync();

        if (application == null) return NotFound();

        return Ok(MapToResponse(application));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateApplicationRequest req)
    {
        var job = await _db.Jobs
            .FirstOrDefaultAsync(j => j.TenantId == _tenant.TenantId && j.Id == req.JobId);

        if (job == null) return NotFound(new { message = "Job not found." });
        if (job.Status != JobStatus.Open)
            return BadRequest(new { message = "Job is not open for applications." });

        var candidate = await _db.Candidates
            .FirstOrDefaultAsync(c => c.TenantId == _tenant.TenantId && c.Email == req.CandidateEmail);

        if (candidate == null)
        {
            candidate = new Candidate
            {
                TenantId = _tenant.TenantId,
                Name = req.CandidateName,
                Email = req.CandidateEmail,
                Phone = req.CandidatePhone
            };
            _db.Candidates.Add(candidate);
            await _db.SaveChangesAsync();
        }

        var existing = await _db.Applications
            .AnyAsync(a => a.JobId == req.JobId && a.CandidateId == candidate.Id);

        if (existing)
            return Conflict(new { message = "Candidate has already applied for this job." });

        var application = new Application
        {
            TenantId = _tenant.TenantId,
            JobId = req.JobId,
            CandidateId = candidate.Id,
            CoverNote = req.CoverNote,
            Stage = ApplicationStage.Applied
        };

        _db.Applications.Add(application);

        _db.ApplicationEvents.Add(new ApplicationEvent
        {
            ApplicationId = application.Id,
            ActorId = _tenant.UserId,
            FromStage = null,
            ToStage = ApplicationStage.Applied,
            Note = "Application received"
        });

        await _db.SaveChangesAsync();

        await _db.Entry(application).Reference(a => a.Job).LoadAsync();
        await _db.Entry(application).Reference(a => a.Candidate).LoadAsync();
        await _db.Entry(application).Collection(a => a.Events).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = application.Id }, MapToResponse(application));
    }

    [HttpPut("{id}/stage")]
    public async Task<IActionResult> MoveStage(Guid id, MoveStageRequest req)
    {
        var application = await _db.Applications
            .Where(a => a.TenantId == _tenant.TenantId && a.Id == id)
            .Include(a => a.Job)
            .Include(a => a.Candidate)
            .Include(a => a.Events)
            .FirstOrDefaultAsync();

        if (application == null) return NotFound();

        if (!Enum.TryParse<ApplicationStage>(req.ToStage, out var toStage))
            return BadRequest(new { message = "Invalid stage value." });

        if (!ApplicationStateMachine.CanTransition(application.Stage, toStage))
        {
            var allowed = ApplicationStateMachine.GetAllowedTransitions(application.Stage);
            return UnprocessableEntity(new
            {
                message = $"Cannot transition from {application.Stage} to {toStage}.",
                allowedTransitions = allowed.Select(s => s.ToString())
            });
        }

        var fromStage = application.Stage;
        application.Stage = toStage;
        application.UpdatedAt = DateTime.UtcNow;

        _db.ApplicationEvents.Add(new ApplicationEvent
        {
            ApplicationId = application.Id,
            ActorId = _tenant.UserId,
            FromStage = fromStage,
            ToStage = toStage,
            Note = req.Note
        });

        await _db.SaveChangesAsync();

        var response = MapToResponse(application);

        await _hub.Clients.Group($"job-{application.JobId}")
            .SendAsync("StageChanged", new
            {
                applicationId = application.Id,
                jobId = application.JobId,
                candidateName = application.Candidate?.Name,
                fromStage = fromStage.ToString(),
                toStage = toStage.ToString()
            });

        return Ok(response);
    }

    private static ApplicationResponse MapToResponse(Application a) => new(
        a.Id,
        a.JobId,
        a.Job?.Title ?? string.Empty,
        a.CandidateId,
        a.Candidate?.Name ?? string.Empty,
        a.Candidate?.Email ?? string.Empty,
        a.Stage.ToString(),
        a.CoverNote,
        a.CreatedAt,
        a.UpdatedAt,
        a.Events.OrderBy(e => e.CreatedAt).Select(e => new StageEventResponse(
            e.FromStage?.ToString(),
            e.ToStage.ToString(),
            e.Note,
            e.CreatedAt
        ))
    );
}
