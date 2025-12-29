using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Data;
using KamuKoprusu.Models;
using KamuKoprusu.Enums;

namespace KamuKoprusu.Controllers.Api;

/// <summary>
/// REST API for Complaints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ComplaintsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ComplaintsApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Get all public complaints (resolved ones)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetComplaints(
        [FromQuery] string? status = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Complaints
            .Include(c => c.Institution)
            .Where(c => c.Status == ComplaintStatus.Resolved || c.Status == ComplaintStatus.InProgress)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ComplaintStatus>(status, out var statusEnum))
        {
            query = query.Where(c => c.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(c => c.Category == category);
        }

        var total = await query.CountAsync();
        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ComplaintDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description.Length > 200 ? c.Description.Substring(0, 200) + "..." : c.Description,
                Type = c.Type.ToString(),
                Category = c.Category,
                Status = c.Status.ToString(),
                InstitutionName = c.Institution != null ? c.Institution.Name : null,
                Location = c.Location,
                CreatedAt = c.CreatedAt,
                ResolvedAt = c.ResolvedAt
            })
            .ToListAsync();

        return Ok(new
        {
            data = complaints,
            pagination = new
            {
                page,
                pageSize,
                total,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            }
        });
    }

    /// <summary>
    /// Get a specific complaint by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ComplaintDetailDto>> GetComplaint(int id)
    {
        var complaint = await _context.Complaints
            .Include(c => c.Institution)
            .Include(c => c.Updates)
            .Where(c => c.Id == id && (c.Status == ComplaintStatus.Resolved || c.Status == ComplaintStatus.InProgress))
            .FirstOrDefaultAsync();

        if (complaint == null)
        {
            return NotFound(new { error = "Complaint not found or not accessible" });
        }

        var dto = new ComplaintDetailDto
        {
            Id = complaint.Id,
            Title = complaint.Title,
            Description = complaint.Description,
            Type = complaint.Type.ToString(),
            Category = complaint.Category,
            Status = complaint.Status.ToString(),
            InstitutionName = complaint.Institution?.Name,
            Location = complaint.Location,
            CreatedAt = complaint.CreatedAt,
            ResolvedAt = complaint.ResolvedAt,
            UpdateCount = complaint.Updates?.Count ?? 0
        };

        return Ok(dto);
    }

    /// <summary>
    /// Get complaint statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var stats = new
        {
            total = await _context.Complaints.CountAsync(),
            resolved = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Resolved),
            inProgress = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.InProgress),
            pending = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.New || c.Status == ComplaintStatus.PendingModeration),
            rejected = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Rejected)
        };

        return Ok(stats);
    }

    /// <summary>
    /// Get complaints by category
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _context.Complaints
            .Where(c => c.Status != ComplaintStatus.Rejected && c.Status != ComplaintStatus.Closed)
            .GroupBy(c => c.Category)
            .Select(g => new { category = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Create a new complaint (requires authentication)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Citizen")]
    public async Task<ActionResult<ComplaintDto>> CreateComplaint([FromBody] CreateComplaintDto model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var institution = await _context.Institutions.FindAsync(model.InstitutionId);
        if (institution == null)
        {
            return BadRequest(new { error = "Institution not found" });
        }

        if (!Enum.TryParse<ComplaintType>(model.Type, out var complaintType))
        {
            return BadRequest(new { error = "Invalid complaint type" });
        }

        var complaint = new Complaint
        {
            Title = model.Title,
            Description = model.Description,
            Type = complaintType,
            Category = model.Category,
            InstitutionId = model.InstitutionId,
            Location = model.Location,
            UserId = user.Id,
            Status = ComplaintStatus.PendingModeration,
            CreatedAt = DateTime.UtcNow
        };

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();

        var dto = new ComplaintDto
        {
            Id = complaint.Id,
            Title = complaint.Title,
            Description = complaint.Description,
            Type = complaint.Type.ToString(),
            Category = complaint.Category,
            Status = complaint.Status.ToString(),
            InstitutionName = institution.Name,
            CreatedAt = complaint.CreatedAt
        };

        return CreatedAtAction(nameof(GetComplaint), new { id = complaint.Id }, dto);
    }
}

// DTOs
public class ComplaintDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? InstitutionName { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class ComplaintDetailDto : ComplaintDto
{
    public int UpdateCount { get; set; }
}

public class CreateComplaintDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int InstitutionId { get; set; }
    public string? Location { get; set; }
}
