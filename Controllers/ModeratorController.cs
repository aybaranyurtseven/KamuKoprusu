using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Models;
using KamuKoprusu.Data;
using KamuKoprusu.Enums;

namespace KamuKoprusu.Controllers;

[Authorize(Roles = "Moderator,Admin")]
public class ModeratorController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ModeratorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var moderatorId = _userManager.GetUserId(User);

        var stats = new
        {
            PendingReview = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.PendingModeration),
            ApprovedToday = await _context.Complaints.CountAsync(c => 
                c.Status != ComplaintStatus.PendingModeration && 
                c.Status != ComplaintStatus.Rejected && 
                c.CreatedAt.Date == DateTime.UtcNow.Date),
            RejectedTotal = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Rejected),
            WarningsIssued = await _context.Warnings.CountAsync(w => w.IssuedByUserId == moderatorId),
            BannedUsers = await _context.BannedUsers.CountAsync()
        };

        var pendingComplaints = await _context.Complaints
            .Where(c => c.Status == ComplaintStatus.PendingModeration)
            .Include(c => c.User)
            .Include(c => c.Institution)
            .OrderBy(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();

        var recentWarnings = await _context.Warnings
            .Include(w => w.User)
            .OrderByDescending(w => w.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.Stats = stats;
        ViewBag.PendingComplaints = pendingComplaints;
        ViewBag.RecentWarnings = recentWarnings;

        return View();
    }

    public async Task<IActionResult> ReviewContent(string? filter)
    {
        var query = _context.Complaints
            .Include(c => c.User)
            .Include(c => c.Institution)
            .Include(c => c.MediaFiles)
            .AsQueryable();

        if (filter == "pending" || string.IsNullOrEmpty(filter))
        {
            query = query.Where(c => c.Status == ComplaintStatus.PendingModeration);
        }
        else if (filter == "rejected")
        {
            query = query.Where(c => c.Status == ComplaintStatus.Rejected);
        }

        var complaints = await query.OrderBy(c => c.CreatedAt).ToListAsync();

        ViewBag.Filter = filter ?? "pending";
        return View(complaints);
    }

    public async Task<IActionResult> ReviewDetail(int id)
    {
        var complaint = await _context.Complaints
            .Include(c => c.User)
            .Include(c => c.Institution)
            .Include(c => c.MediaFiles)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (complaint == null)
        {
            return NotFound();
        }

        // Get user warning history
        if (complaint.UserId != null)
        {
            ViewBag.UserWarnings = await _context.Warnings
                .Where(w => w.UserId == complaint.UserId)
                .OrderByDescending(w => w.CreatedAt)
                .ToListAsync();
        }

        return View(complaint);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveComplaint(int id)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null)
        {
            return NotFound();
        }

        complaint.Status = ComplaintStatus.New;
        complaint.IsApproved = true;

        // Add update record
        var update = new ComplaintUpdate
        {
            ComplaintId = id,
            Message = "Başvuru moderatör tarafından onaylandı.",
            NewStatus = ComplaintStatus.New,
            UpdatedByUserId = _userManager.GetUserId(User),
            CreatedAt = DateTime.UtcNow
        };
        _context.ComplaintUpdates.Add(update);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Başvuru onaylandı ve kuruma yönlendirildi.";
        return RedirectToAction(nameof(ReviewContent));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectComplaint(int id, string reason)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null)
        {
            return NotFound();
        }

        complaint.Status = ComplaintStatus.Rejected;
        complaint.IsApproved = false;
        complaint.RejectionReason = reason;

        // Add update record
        var update = new ComplaintUpdate
        {
            ComplaintId = id,
            Message = $"Başvuru reddedildi. Neden: {reason}",
            NewStatus = ComplaintStatus.Rejected,
            UpdatedByUserId = _userManager.GetUserId(User),
            CreatedAt = DateTime.UtcNow
        };
        _context.ComplaintUpdates.Add(update);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Başvuru reddedildi.";
        return RedirectToAction(nameof(ReviewContent));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WarnUser(string userId, int? complaintId, string reason)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(ReviewContent));
        }

        // Create warning
        var warning = new Warning
        {
            UserId = userId,
            Reason = reason,
            ComplaintId = complaintId,
            IssuedByUserId = _userManager.GetUserId(User)!,
            CreatedAt = DateTime.UtcNow
        };
        _context.Warnings.Add(warning);

        // Check warning count
        var warningCount = await _context.Warnings.CountAsync(w => w.UserId == userId) + 1;

        string message;
        if (warningCount >= 4)
        {
            // Permanent ban
            user.IsBanned = true;
            var bannedUser = new BannedUser
            {
                UserId = userId,
                Reason = "4 uyarı nedeniyle kalıcı yasaklama",
                BannedByUserId = _userManager.GetUserId(User)!,
                IsPermanent = true,
                BannedEmail = user.Email,
                BannedPhone = user.PhoneNumber
            };
            _context.BannedUsers.Add(bannedUser);
            await _userManager.UpdateAsync(user);
            message = $"{user.FullName} 4. uyarı nedeniyle kalıcı olarak yasaklandı.";
        }
        else if (warningCount == 3)
        {
            // 30 day suspension
            user.IsBanned = true;
            var bannedUser = new BannedUser
            {
                UserId = userId,
                Reason = "3. uyarı: 30 gün askıya alma",
                BannedByUserId = _userManager.GetUserId(User)!,
                IsPermanent = false,
                BanExpiresAt = DateTime.UtcNow.AddDays(30),
                BannedEmail = user.Email,
                BannedPhone = user.PhoneNumber
            };
            _context.BannedUsers.Add(bannedUser);
            await _userManager.UpdateAsync(user);
            message = $"{user.FullName} 30 gün askıya alındı (3. uyarı).";
        }
        else if (warningCount == 2)
        {
            // 7 day suspension
            user.IsBanned = true;
            var bannedUser = new BannedUser
            {
                UserId = userId,
                Reason = "2. uyarı: 7 gün askıya alma",
                BannedByUserId = _userManager.GetUserId(User)!,
                IsPermanent = false,
                BanExpiresAt = DateTime.UtcNow.AddDays(7),
                BannedEmail = user.Email,
                BannedPhone = user.PhoneNumber
            };
            _context.BannedUsers.Add(bannedUser);
            await _userManager.UpdateAsync(user);
            message = $"{user.FullName} 7 gün askıya alındı (2. uyarı).";
        }
        else
        {
            message = $"{user.FullName} uyarıldı (1. uyarı).";
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = message;
        
        if (complaintId.HasValue)
        {
            return RedirectToAction(nameof(ReviewDetail), new { id = complaintId });
        }
        return RedirectToAction(nameof(WarningManagement));
    }

    public async Task<IActionResult> WarningManagement()
    {
        var usersWithWarnings = await _context.Warnings
            .Include(w => w.User)
            .GroupBy(w => w.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                User = g.First().User,
                WarningCount = g.Count(),
                FirstWarning = g.Min(w => w.CreatedAt),
                LastWarning = g.Max(w => w.CreatedAt)
            })
            .OrderByDescending(x => x.WarningCount)
            .ToListAsync();

        ViewBag.UsersWithWarnings = usersWithWarnings;

        return View();
    }
}
