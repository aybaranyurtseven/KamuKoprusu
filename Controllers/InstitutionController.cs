using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Models;
using KamuKoprusu.Enums;
using KamuKoprusu.Data;
using KamuKoprusu.Services;

namespace KamuKoprusu.Controllers;

[Authorize(Roles = "InstitutionRepresentative")]
public class InstitutionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGamificationService _gamificationService;

    public InstitutionController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IGamificationService gamificationService)
    {
        _context = context;
        _userManager = userManager;
        _gamificationService = gamificationService;
    }

    // Redirect old Dashboard to Profile
    public IActionResult Dashboard()
    {
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> Complaints(string? status, string? search)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user?.InstitutionId == null)
        {
            return Content("Hesabınız bir kuruma bağlı değil.");
        }

        var institutionId = user.InstitutionId.Value;

        var query = _context.Complaints
            .Where(c => c.InstitutionId == institutionId && c.Status != ComplaintStatus.PendingModeration)
            .Include(c => c.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ComplaintStatus>(status, out var statusEnum))
        {
            query = query.Where(c => c.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
        }

        var complaints = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.Search = search;

        return View(complaints);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var complaint = await _context.Complaints
            .Include(c => c.User)
            .Include(c => c.Institution)
            .Include(c => c.MediaFiles)
            .Include(c => c.Updates)
                .ThenInclude(u => u.UpdatedBy)
            .Include(c => c.Updates)
                .ThenInclude(u => u.MediaFiles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (complaint == null)
        {
            return NotFound();
        }

        return View(complaint);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int complaintId, ComplaintStatus newStatus, string message)
    {
        var complaint = await _context.Complaints.FindAsync(complaintId);
        if (complaint == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);

        var update = new ComplaintUpdate
        {
            ComplaintId = complaintId,
            Message = message,
            NewStatus = newStatus,
            UpdatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        complaint.Status = newStatus;
        
        if (newStatus == ComplaintStatus.Resolved)
        {
            complaint.ResolvedAt = DateTime.UtcNow;
        }

        _context.ComplaintUpdates.Add(update);
        await _context.SaveChangesAsync();

        // Check and award badges when complaint is resolved
        if (newStatus == ComplaintStatus.Resolved && !string.IsNullOrEmpty(complaint.UserId))
        {
            await _gamificationService.CheckAndAwardBadgesAsync(complaint.UserId);
        }

        TempData["Success"] = "Şikayet durumu güncellendi.";
        return RedirectToAction(nameof(Detail), new { id = complaintId });
    }

    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user?.InstitutionId == null)
        {
            return Content("Hesabınız bir kuruma bağlı değil.");
        }

        var institution = await _context.Institutions.FindAsync(user.InstitutionId.Value);
        if (institution == null)
        {
            return NotFound();
        }

        // Get institution stats
        var stats = new
        {
            TotalComplaints = await _context.Complaints.CountAsync(c => c.InstitutionId == institution.Id),
            ResolvedComplaints = await _context.Complaints.CountAsync(c => c.InstitutionId == institution.Id && c.Status == ComplaintStatus.Resolved),
            PendingComplaints = await _context.Complaints.CountAsync(c => c.InstitutionId == institution.Id && c.Status == ComplaintStatus.New),
            InProgressComplaints = await _context.Complaints.CountAsync(c => c.InstitutionId == institution.Id && c.Status == ComplaintStatus.InProgress),
            RepresentativeCount = await _context.Users.CountAsync(u => u.InstitutionId == institution.Id)
        };

        // Calculate resolution rate
        var resolutionRate = stats.TotalComplaints > 0 
            ? (stats.ResolvedComplaints * 100 / stats.TotalComplaints) 
            : 0;

        // Get recent complaints
        var recentComplaints = await _context.Complaints
            .Where(c => c.InstitutionId == institution.Id)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();

        ViewBag.Stats = stats;
        ViewBag.ResolutionRate = resolutionRate;
        ViewBag.RecentComplaints = recentComplaints;

        return View(institution);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user?.InstitutionId == null)
        {
            return Content("Hesabınız bir kuruma bağlı değil.");
        }

        var institution = await _context.Institutions.FindAsync(user.InstitutionId.Value);
        if (institution == null)
        {
            return NotFound();
        }

        return View(institution);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(Institution model)
    {
        var user = await _userManager.GetUserAsync(User);
        
        if (user?.InstitutionId == null)
        {
            return Content("Hesabınız bir kuruma bağlı değil.");
        }

        var institution = await _context.Institutions.FindAsync(user.InstitutionId.Value);
        if (institution == null)
        {
            return NotFound();
        }

        // Update only allowed fields (not Name or InstitutionCode)
        institution.Address = model.Address;
        institution.Phone = model.Phone;
        institution.Email = model.Email;
        institution.Website = model.Website;
        institution.About = model.About;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Kurum bilgileri başarıyla güncellendi.";
        return RedirectToAction(nameof(Profile));
    }
}
