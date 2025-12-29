using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Models;
using KamuKoprusu.Data;
using KamuKoprusu.Enums;

namespace KamuKoprusu.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public AdminController(
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var pendingApprovals = await _userManager.Users.CountAsync(u => !u.IsApproved);
        var bannedUsers = await _userManager.Users.CountAsync(u => u.IsBanned);
        var totalComplaints = await _context.Complaints.CountAsync();
        var resolvedComplaints = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Resolved);
        var pendingModeration = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.PendingModeration);
        var totalInstitutions = await _context.Institutions.CountAsync();
        var moderators = await _userManager.GetUsersInRoleAsync("Moderator");
        var recentBans = await _context.BannedUsers.CountAsync(b => b.BannedAt >= DateTime.UtcNow.AddDays(-1));

        var stats = new
        {
            TotalUsers = totalUsers,
            PendingApprovals = pendingApprovals,
            BannedUsers = bannedUsers,
            TotalComplaints = totalComplaints,
            ResolvedComplaints = resolvedComplaints,
            PendingModeration = pendingModeration,
            TotalInstitutions = totalInstitutions,
            ModeratorCount = moderators.Count,
            RecentBans = recentBans,
            ResolutionRate = totalComplaints > 0 ? (resolvedComplaints * 100 / totalComplaints) : 0
        };

        ViewBag.Stats = stats;
        return View();
    }

    public async Task<IActionResult> Users(string? search, string? statusFilter, string? roleFilter)
    {
        var query = _context.Users
            .Include(u => u.Institution)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FullName!.Contains(search) || u.Email!.Contains(search));
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (statusFilter == "pending")
                query = query.Where(u => !u.IsApproved);
            else if (statusFilter == "banned")
                query = query.Where(u => u.IsBanned);
            else if (statusFilter == "active")
                query = query.Where(u => u.IsApproved && !u.IsBanned);
        }

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

        // Filter by role if specified
        if (!string.IsNullOrEmpty(roleFilter))
        {
            var usersWithRole = new List<ApplicationUser>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(roleFilter))
                {
                    usersWithRole.Add(user);
                }
            }
            users = usersWithRole;
        }

        ViewBag.Search = search;
        ViewBag.StatusFilter = statusFilter;
        ViewBag.RoleFilter = roleFilter;

        return View(users);
    }

    // ============ COMPLAINTS MANAGEMENT ============
    public async Task<IActionResult> Complaints(string? status, string? search, int? institutionId)
    {
        var query = _context.Complaints
            .Include(c => c.User)
            .Include(c => c.Institution)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ComplaintStatus>(status, out var statusEnum))
        {
            query = query.Where(c => c.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
        }

        if (institutionId.HasValue)
        {
            query = query.Where(c => c.InstitutionId == institutionId.Value);
        }

        var complaints = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        var institutions = await _context.Institutions.OrderBy(i => i.Name).ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.Search = search;
        ViewBag.InstitutionId = institutionId;
        ViewBag.Institutions = institutions;

        return View(complaints);
    }

    public async Task<IActionResult> ComplaintDetail(int id)
    {
        var complaint = await _context.Complaints
            .Include(c => c.User)
            .Include(c => c.Institution)
            .Include(c => c.MediaFiles)
            .Include(c => c.Updates)
                .ThenInclude(u => u.UpdatedBy)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (complaint == null)
        {
            return NotFound();
        }

        return View(complaint);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteComplaint(int id, string reason)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null)
        {
            return NotFound();
        }

        // Log the deletion
        var auditLog = new AuditLog
        {
            UserId = _userManager.GetUserId(User),
            Action = "Başvuru Silindi",
            EntityType = "Complaint",
            EntityId = id,
            Details = $"Silme nedeni: {reason}",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            CreatedAt = DateTime.UtcNow
        };
        _context.AuditLogs.Add(auditLog);

        _context.Complaints.Remove(complaint);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Başvuru başarıyla silindi.";
        return RedirectToAction(nameof(Complaints));
    }

    // ============ MODERATOR MANAGEMENT ============
    public async Task<IActionResult> ModeratorManagement()
    {
        var moderators = await _userManager.GetUsersInRoleAsync("Moderator");
        
        // Get statistics for each moderator
        var moderatorStats = new List<dynamic>();
        foreach (var moderator in moderators)
        {
            var warningsIssued = await _context.Warnings.CountAsync(w => w.IssuedByUserId == moderator.Id);
            moderatorStats.Add(new
            {
                User = moderator,
                WarningsIssued = warningsIssued
            });
        }

        ViewBag.ModeratorStats = moderatorStats;
        ViewBag.AvailableUsers = await _userManager.Users
            .Where(u => u.IsApproved && !u.IsBanned)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(moderators);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignModerator(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(ModeratorManagement));
        }

        // Check if role exists
        if (!await _roleManager.RoleExistsAsync("Moderator"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Moderator"));
        }

        var result = await _userManager.AddToRoleAsync(user, "Moderator");
        if (result.Succeeded)
        {
            TempData["Success"] = $"{user.FullName} moderatör olarak atandı.";
        }
        else
        {
            TempData["Error"] = "Moderatör atama başarısız.";
        }

        return RedirectToAction(nameof(ModeratorManagement));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveModerator(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Kullanıcı bulunamadı.";
            return RedirectToAction(nameof(ModeratorManagement));
        }

        var result = await _userManager.RemoveFromRoleAsync(user, "Moderator");
        if (result.Succeeded)
        {
            TempData["Success"] = $"{user.FullName} moderatörlükten çıkarıldı.";
        }
        else
        {
            TempData["Error"] = "Moderatör çıkarma başarısız.";
        }

        return RedirectToAction(nameof(ModeratorManagement));
    }

    // ============ REPORTS ============
    public async Task<IActionResult> Reports()
    {
        // Monthly complaint statistics
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var monthlyStats = await _context.Complaints
            .Where(c => c.CreatedAt >= sixMonthsAgo)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
                Resolved = g.Count(c => c.Status == ComplaintStatus.Resolved)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        // Institution statistics
        var institutionStats = await _context.Complaints
            .GroupBy(c => c.Institution)
            .Select(g => new
            {
                Institution = g.Key!.Name,
                Total = g.Count(),
                Resolved = g.Count(c => c.Status == ComplaintStatus.Resolved),
                Pending = g.Count(c => c.Status == ComplaintStatus.New || c.Status == ComplaintStatus.InProgress)
            })
            .OrderByDescending(x => x.Total)
            .Take(10)
            .ToListAsync();

        // Category statistics
        var categoryStats = await _context.Complaints
            .GroupBy(c => c.Type)
            .Select(g => new
            {
                Type = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        // Top active citizens
        var topCitizens = await _context.Complaints
            .Where(c => !c.IsAnonymous)
            .GroupBy(c => c.User)
            .Select(g => new
            {
                User = g.Key!.FullName,
                ComplaintCount = g.Count(),
                ResolvedCount = g.Count(c => c.Status == ComplaintStatus.Resolved)
            })
            .OrderByDescending(x => x.ComplaintCount)
            .Take(10)
            .ToListAsync();

        // Average resolution time
        var resolvedComplaints = await _context.Complaints
            .Where(c => c.Status == ComplaintStatus.Resolved && c.ResolvedAt.HasValue)
            .Select(c => new { c.CreatedAt, c.ResolvedAt })
            .ToListAsync();

        double avgResolutionDays = 0;
        if (resolvedComplaints.Any())
        {
            avgResolutionDays = resolvedComplaints
                .Average(c => (c.ResolvedAt!.Value - c.CreatedAt).TotalDays);
        }

        ViewBag.MonthlyStats = monthlyStats;
        ViewBag.InstitutionStats = institutionStats;
        ViewBag.CategoryStats = categoryStats;
        ViewBag.TopCitizens = topCitizens;
        ViewBag.AvgResolutionDays = Math.Round(avgResolutionDays, 1);

        return View();
    }

    // ============ USER MANAGEMENT (existing) ============
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsApproved = true;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = $"{user.FullName} başarıyla onaylandı.";
        
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer) && referer.Contains("InstitutionApprovals"))
        {
            return RedirectToAction(nameof(InstitutionApprovals));
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(string userId, string reason)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsBanned = true;
        await _userManager.UpdateAsync(user);

        var bannedUser = new BannedUser
        {
            UserId = userId,
            Reason = reason ?? "Kuralları ihlal",
            BannedByUserId = _userManager.GetUserId(User)!,
            IsPermanent = true,
            BannedEmail = user.Email,
            BannedPhone = user.PhoneNumber
        };

        _context.BannedUsers.Add(bannedUser);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"{user.FullName} yasaklandı.";
        
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer) && referer.Contains("InstitutionApprovals"))
        {
            return RedirectToAction(nameof(InstitutionApprovals));
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbanUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.IsBanned = false;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = $"{user.FullName} yasağı kaldırıldı.";
        
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer) && referer.Contains("InstitutionApprovals"))
        {
            return RedirectToAction(nameof(InstitutionApprovals));
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId, string reason)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(user.Email))
        {
            var bannedRecord = new BannedUser
            {
                UserId = user.Id,
                BannedByUserId = _userManager.GetUserId(User)!,
                Reason = reason ?? "Hesap silindi",
                BannedAt = DateTime.UtcNow,
                IsPermanent = true,
                BannedEmail = user.Email,
                BannedPhone = user.PhoneNumber
            };
            _context.BannedUsers.Add(bannedRecord);
        }

        user.IsBanned = true;
        user.Email = $"deleted_{user.Id}@deleted.local";
        user.NormalizedEmail = user.Email.ToUpper();
        user.UserName = $"deleted_{user.Id}";
        user.NormalizedUserName = user.UserName.ToUpper();
        user.PhoneNumber = null;
        user.FullName = "Silinen Kullanıcı";
        
        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Kullanıcı başarıyla silindi ve email/telefon numarası kalıcı olarak engellendi.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> InstitutionApprovals()
    {
        var institutionReps = await _userManager.GetUsersInRoleAsync("InstitutionRepresentative");
        var pendingApprovals = institutionReps.Where(u => !u.IsApproved).ToList();

        return View(pendingApprovals);
    }
}
