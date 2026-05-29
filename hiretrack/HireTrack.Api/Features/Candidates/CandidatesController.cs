using HireTrack.Api.Common;
using HireTrack.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HireTrack.Api.Features.Candidates;

[ApiController]
[Route("api/candidates")]
[Authorize]
public class CandidatesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TenantContext _tenant;

    public CandidatesController(AppDbContext db, TenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var candidates = await _db.Candidates
            .Where(c => c.TenantId == _tenant.TenantId)
            .Include(c => c.Applications)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CandidateResponse(
                c.Id,
                c.Name,
                c.Email,
                c.Phone,
                c.ResumeUrl,
                c.Applications.Count,
                c.CreatedAt
            ))
            .ToListAsync();

        return Ok(candidates);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var candidate = await _db.Candidates
            .Where(c => c.TenantId == _tenant.TenantId && c.Id == id)
            .Include(c => c.Applications)
            .ThenInclude(a => a.Job)
            .FirstOrDefaultAsync();

        if (candidate == null) return NotFound();

        return Ok(new
        {
            candidate.Id,
            candidate.Name,
            candidate.Email,
            candidate.Phone,
            candidate.ResumeUrl,
            candidate.CreatedAt,
            Applications = candidate.Applications.Select(a => new
            {
                a.Id,
                a.JobId,
                JobTitle = a.Job.Title,
                Stage = a.Stage.ToString(),
                a.CreatedAt
            })
        });
    }
}
