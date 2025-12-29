using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Data;
using KamuKoprusu.Models;

namespace KamuKoprusu.Controllers.Api;

/// <summary>
/// REST API for Users (Admin only)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class UsersApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UsersApiController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
        [FromQuery] string? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(role))
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var userIds = usersInRole.Select(u => u.Id);
            query = query.Where(u => userIds.Contains(u.Id));
        }

        var total = await query.CountAsync();
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName ?? "",
                Email = u.Email ?? "",
                PhoneNumber = u.PhoneNumber,
                IsBanned = u.IsBanned,
                IsApproved = u.IsApproved,
                CreatedAt = u.CreatedAt,
                InstitutionId = u.InstitutionId
            })
            .ToListAsync();

        return Ok(new
        {
            data = users,
            pagination = new { page, pageSize, total, totalPages = (int)Math.Ceiling(total / (double)pageSize) }
        });
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(string id)
    {
        var user = await _context.Users
            .Include(u => u.Institution)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new UserDetailDto
        {
            Id = user.Id,
            FullName = user.FullName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            IsBanned = user.IsBanned,
            IsApproved = user.IsApproved,
            CreatedAt = user.CreatedAt,
            InstitutionId = user.InstitutionId,
            InstitutionName = user.Institution?.Name,
            Roles = roles.ToList()
        };

        return Ok(dto);
    }

    /// <summary>
    /// Get user statistics (Admin only)
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var stats = new
        {
            totalUsers = await _context.Users.CountAsync(),
            bannedUsers = await _context.Users.CountAsync(u => u.IsBanned),
            newThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
            citizens = (await _userManager.GetUsersInRoleAsync("Citizen")).Count,
            institutionReps = (await _userManager.GetUsersInRoleAsync("InstitutionRepresentative")).Count,
            moderators = (await _userManager.GetUsersInRoleAsync("Moderator")).Count
        };

        return Ok(stats);
    }

    /// <summary>
    /// Toggle user ban status (Admin only)
    /// </summary>
    [HttpPatch("{id}/toggle-ban")]
    public async Task<ActionResult> ToggleUserBan(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        user.IsBanned = !user.IsBanned;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "User ban status updated", isBanned = user.IsBanned });
    }
}

// DTOs
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsBanned { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? InstitutionId { get; set; }
}

public class UserDetailDto : UserDto
{
    public string? InstitutionName { get; set; }
    public List<string> Roles { get; set; } = new();
}
