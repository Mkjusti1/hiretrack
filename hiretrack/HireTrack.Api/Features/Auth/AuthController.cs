using HireTrack.Api.Common;
using HireTrack.Api.Data;
using HireTrack.Api.Domain.Entities;
using HireTrack.Api.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HireTrack.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<AppUser> userManager, AppDbContext db, TokenService tokenService)
    {
        _userManager = userManager;
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (await _db.Tenants.AnyAsync(t => t.Slug == req.CompanyName.ToLower().Replace(" ", "-")))
            return Conflict(new { message = "A company with this name already exists." });

        var tenant = new Tenant
        {
            Name = req.CompanyName,
            Slug = req.CompanyName.ToLower().Replace(" ", "-")
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        var user = new AppUser
        {
            UserName = req.Email,
            Email = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            TenantId = tenant.Id,
            Role = UserRole.Owner
        };

        var result = await _userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync();
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponse(
            token,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.TenantId
        ));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponse(
            token,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.TenantId
        ));
    }
}
