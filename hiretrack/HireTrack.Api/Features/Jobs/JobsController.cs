using HireTrack.Api.Common;
using HireTrack.Api.Data;
using HireTrack.Api.Domain.Entities;
using HireTrack.Api.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireTrack.Api.Features.Jobs;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TenantContext _tenant;

    public JobsController(AppDbContext db, TenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var jobs = await _db.Jobs
            .Where(j => j.TenantId == _tenant.TenantId)
            .Include(j => j.Applications)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobResponse(
                j.Id,
                j.Title,
                j.Department,
                j.Location,
                j.Description,
                j.Status.ToString(),
                j.Applications.Count,
                j.CreatedAt
            ))
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var job = await _db.Jobs
            .Where(j => j.TenantId == _tenant.TenantId && j.Id == id)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync();

        if (job == null) return NotFound();

        return Ok(new JobResponse(
            job.Id,
            job.Title,
            job.Department,
            job.Location,
            job.Description,
            job.Status.ToString(),
            job.Applications.Count,
            job.CreatedAt
        ));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateJobRequest req)
    {
        var job = new Job
        {
            TenantId = _tenant.TenantId,
            CreatedById = _tenant.UserId,
            Title = req.Title,
            Department = req.Department,
            Location = req.Location,
            Description = req.Description
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = job.Id }, new JobResponse(
            job.Id,
            job.Title,
            job.Department,
            job.Location,
            job.Description,
            job.Status.ToString(),
            0,
            job.CreatedAt
        ));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateJobRequest req)
    {
        var job = await _db.Jobs
            .FirstOrDefaultAsync(j => j.TenantId == _tenant.TenantId && j.Id == id);

        if (job == null) return NotFound();

        if (!Enum.TryParse<JobStatus>(req.Status, out var status))
            return BadRequest(new { message = "Invalid status value." });

        job.Title = req.Title;
        job.Department = req.Department;
        job.Location = req.Location;
        job.Description = req.Description;
        job.Status = status;
        job.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var job = await _db.Jobs
            .FirstOrDefaultAsync(j => j.TenantId == _tenant.TenantId && j.Id == id);

        if (job == null) return NotFound();

        job.Status = JobStatus.Archived;
        job.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return NoContent();
    }
    [HttpPut("{id}/unarchive")]
public async Task<IActionResult> Unarchive(Guid id)
{
    var job = await _db.Jobs
        .FirstOrDefaultAsync(j => j.TenantId == _tenant.TenantId && j.Id == id);

    if (job == null) return NotFound();

    job.Status = JobStatus.Open;
    job.UpdatedAt = DateTime.UtcNow;
    await _db.SaveChangesAsync();

    return NoContent();
}

[HttpDelete("{id}/permanent")]
public async Task<IActionResult> DeletePermanent(Guid id)
{
    var job = await _db.Jobs
        .FirstOrDefaultAsync(j => j.TenantId == _tenant.TenantId && j.Id == id);

    if (job == null) return NotFound();

    _db.Jobs.Remove(job);
    await _db.SaveChangesAsync();

    return NoContent();
}
}
