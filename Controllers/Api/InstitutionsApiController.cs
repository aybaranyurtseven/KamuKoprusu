using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Data;

namespace KamuKoprusu.Controllers.Api;

/// <summary>
/// REST API for Institutions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InstitutionsApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InstitutionsApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all institutions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InstitutionDto>>> GetInstitutions(
        [FromQuery] string? type = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Institutions.AsQueryable();

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(i => i.Type == type);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => i.Name.Contains(search));
        }

        var institutions = await query
            .OrderBy(i => i.Name)
            .Select(i => new InstitutionDto
            {
                Id = i.Id,
                Name = i.Name,
                Type = i.Type,
                Email = i.Email,
                Phone = i.Phone,
                Address = i.Address,
                Website = i.Website,
                LogoUrl = i.LogoUrl
            })
            .ToListAsync();

        return Ok(institutions);
    }

    /// <summary>
    /// Get a specific institution by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InstitutionDetailDto>> GetInstitution(int id)
    {
        var institution = await _context.Institutions
            .Include(i => i.Complaints)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (institution == null)
        {
            return NotFound(new { error = "Institution not found" });
        }

        var dto = new InstitutionDetailDto
        {
            Id = institution.Id,
            Name = institution.Name,
            Type = institution.Type,
            Email = institution.Email,
            Phone = institution.Phone,
            Address = institution.Address,
            Website = institution.Website,
            About = institution.About,
            LogoUrl = institution.LogoUrl,
            TotalComplaints = institution.Complaints?.Count ?? 0,
            ResolvedComplaints = institution.Complaints?.Count(c => c.Status == Enums.ComplaintStatus.Resolved) ?? 0
        };

        return Ok(dto);
    }

    /// <summary>
    /// Get institution types
    /// </summary>
    [HttpGet("types")]
    public async Task<ActionResult> GetTypes()
    {
        var types = await _context.Institutions
            .GroupBy(i => i.Type)
            .Select(g => new { type = g.Key, count = g.Count() })
            .ToListAsync();

        return Ok(types);
    }

    /// <summary>
    /// Get institution stats
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var stats = new
        {
            totalInstitutions = await _context.Institutions.CountAsync(),
            types = await _context.Institutions.Select(i => i.Type).Distinct().CountAsync(),
            withComplaints = await _context.Institutions.CountAsync(i => i.Complaints.Any())
        };

        return Ok(stats);
    }
}

// DTOs
public class InstitutionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
}

public class InstitutionDetailDto : InstitutionDto
{
    public string? About { get; set; }
    public int TotalComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
}
