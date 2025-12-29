using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Models;
using KamuKoprusu.Enums;
using KamuKoprusu.Data;
using KamuKoprusu.Services;

namespace KamuKoprusu.Controllers;

[Authorize(Roles = "Citizen")]
public class CitizenController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly IGamificationService _gamificationService;
    private readonly IAuditService _auditService;

    public CitizenController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment,
        IGamificationService gamificationService,
        IAuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
        _gamificationService = gamificationService;
        _auditService = auditService;
    }

    // Redirect old Dashboard to Profile
    public IActionResult Dashboard()
    {
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> MyComplaints(string? status, string? search, string? category, DateTime? startDate, DateTime? endDate)
    {
        var userId = _userManager.GetUserId(User);
        var query = _context.Complaints
            .Where(c => c.UserId == userId)
            .Include(c => c.Institution)
            .AsQueryable();

        // Status filter
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ComplaintStatus>(status, out var statusEnum))
        {
            query = query.Where(c => c.Status == statusEnum);
        }

        // Search filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
        }

        // Category filter
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(c => c.Category == category);
        }

        // Date range filter
        if (startDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= endDate.Value.AddDays(1));
        }

        var complaints = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        // Get distinct categories for filter dropdown
        var categories = await _context.Complaints
            .Where(c => c.UserId == userId)
            .Select(c => c.Category)
            .Distinct()
            .ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.Search = search;
        ViewBag.CategoryFilter = category;
        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.Categories = categories;

        return View(complaints);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var institutions = await _context.Institutions
            .OrderBy(i => i.Name)
            .ToListAsync();

        ViewBag.Institutions = institutions;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateComplaintViewModel model, List<IFormFile>? files)
    {
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);

            var complaint = new Complaint
            {
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                Category = model.Category,
                Location = model.Location,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                IsAnonymous = model.IsAnonymous,
                UserId = userId!,
                InstitutionId = model.InstitutionId,
                Status = ComplaintStatus.PendingModeration,
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            // Handle file uploads
            if (files != null && files.Any())
            {
                var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "complaints", complaint.Id.ToString());
                Directory.CreateDirectory(uploadPath);

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var mediaType = GetMediaType(file.ContentType);
                        var media = new Media
                        {
                            Type = mediaType,
                            FileName = file.FileName,
                            FilePath = $"/uploads/complaints/{complaint.Id}/{fileName}",
                            FileSizeBytes = file.Length,
                            ComplaintId = complaint.Id,
                            UploadedAt = DateTime.UtcNow
                        };

                        _context.MediaFiles.Add(media);
                    }
                }

                await _context.SaveChangesAsync();
            }

            // Check and award badges after submission
            await _gamificationService.CheckAndAwardBadgesAsync(userId!);

            // Audit log
            await _auditService.LogAsync(userId!, "ComplaintCreated", "Complaint", complaint.Id, 
                $"Title: {complaint.Title}, Institution: {complaint.InstitutionId}");

            TempData["Success"] = "Şikayetiniz başarıyla gönderildi. Moderatör onayından sonra yayınlanacaktır.";
            return RedirectToAction(nameof(Detail), new { id = complaint.Id });
        }

        var institutions = await _context.Institutions.OrderBy(i => i.Name).ToListAsync();
        ViewBag.Institutions = institutions;
        return View(model);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var userId = _userManager.GetUserId(User);
        
        var complaint = await _context.Complaints
            .Include(c => c.Institution)
            .Include(c => c.MediaFiles)
            .Include(c => c.Updates)
                .ThenInclude(u => u.UpdatedBy)
            .Include(c => c.Updates)
                .ThenInclude(u => u.MediaFiles)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (complaint == null)
        {
            return NotFound();
        }

        return View(complaint);
    }

    private MediaType GetMediaType(string contentType)
    {
        if (contentType.StartsWith("image/"))
            return MediaType.Photo;
        else if (contentType.StartsWith("video/"))
            return MediaType.Video;
        else if (contentType.StartsWith("audio/"))
            return MediaType.Audio;
        
        return MediaType.Photo; // default
    }

    public async Task<IActionResult> Profile()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.Users
            .Include(u => u.Profile)
            .Include(u => u.UserBadges)
                .ThenInclude(ub => ub.Badge)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        // Get stats (merged from Dashboard)
        var stats = new
        {
            TotalComplaints = await _context.Complaints.CountAsync(c => c.UserId == userId),
            PendingComplaints = await _context.Complaints.CountAsync(c => c.UserId == userId && c.Status == ComplaintStatus.New),
            ResolvedComplaints = await _context.Complaints.CountAsync(c => c.UserId == userId && c.Status == ComplaintStatus.Resolved),
            InProgressComplaints = await _context.Complaints.CountAsync(c => c.UserId == userId && c.Status == ComplaintStatus.InProgress),
            Warnings = await _context.Warnings.CountAsync(w => w.UserId == userId)
        };

        // Recent complaints (from Dashboard)
        var recentComplaints = await _context.Complaints
            .Where(c => c.UserId == userId)
            .Include(c => c.Institution)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.Stats = stats;
        ViewBag.RecentComplaints = recentComplaints;

        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var userId = _userManager.GetUserId(User);
        var user = await _userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        var model = new EditProfileViewModel
        {
            FullName = user.FullName ?? "",
            PhoneNumber = user.PhoneNumber,
            Bio = user.Profile?.Bio,
            City = user.Profile?.City,
            District = user.Profile?.District,
            TwitterUrl = user.Profile?.TwitterUrl,
            LinkedInUrl = user.Profile?.LinkedInUrl,
            InstagramUrl = user.Profile?.InstagramUrl
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        var user = await _userManager.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        // Update user
        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;
        await _userManager.UpdateAsync(user);

        // Update or create profile
        if (user.Profile == null)
        {
            user.Profile = new Profile
            {
                UserId = userId!
            };
            _context.Profiles.Add(user.Profile);
        }

        user.Profile.Bio = model.Bio;
        user.Profile.City = model.City;
        user.Profile.District = model.District;
        user.Profile.TwitterUrl = model.TwitterUrl;
        user.Profile.LinkedInUrl = model.LinkedInUrl;
        user.Profile.InstagramUrl = model.InstagramUrl;
        user.Profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Profiliniz başarıyla güncellendi.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelComplaint(int id)
    {
        var userId = _userManager.GetUserId(User);
        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (complaint == null)
        {
            return NotFound();
        }

        // Only allow cancellation for pending moderation or new complaints
        if (complaint.Status != ComplaintStatus.PendingModeration && complaint.Status != ComplaintStatus.New)
        {
            TempData["Error"] = "Bu aşamada başvuruyu iptal edemezsiniz.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        complaint.Status = ComplaintStatus.Closed;
        
        var update = new ComplaintUpdate
        {
            ComplaintId = id,
            Message = "Başvuru vatandaş tarafından iptal edildi.",
            NewStatus = ComplaintStatus.Closed,
            UpdatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _context.ComplaintUpdates.Add(update);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Başvurunuz iptal edildi.";
        return RedirectToAction(nameof(MyComplaints));
    }

    public async Task<IActionResult> Achievements()
    {
        var userId = _userManager.GetUserId(User);
        
        var userBadges = await _context.UserBadges
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .ToListAsync();

        var allBadges = await _context.Badges.ToListAsync();

        ViewBag.UserBadges = userBadges;
        ViewBag.AllBadges = allBadges;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        
        var complaint = await _context.Complaints
            .Include(c => c.Institution)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (complaint == null)
        {
            return NotFound();
        }

        // Only allow editing if complaint is pending moderation
        if (complaint.Status != ComplaintStatus.PendingModeration)
        {
            TempData["Error"] = "Sadece moderatör onayı bekleyen başvurular düzenlenebilir.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var model = new EditComplaintViewModel
        {
            Id = complaint.Id,
            Title = complaint.Title,
            Description = complaint.Description,
            Type = complaint.Type,
            Category = complaint.Category,
            Location = complaint.Location,
            CurrentStatus = complaint.Status,
            InstitutionName = complaint.Institution?.Name ?? ""
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditComplaintViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        
        var complaint = await _context.Complaints
            .Include(c => c.Institution)
            .FirstOrDefaultAsync(c => c.Id == model.Id && c.UserId == userId);

        if (complaint == null)
        {
            return NotFound();
        }

        // Only allow editing if complaint is pending moderation
        if (complaint.Status != ComplaintStatus.PendingModeration)
        {
            TempData["Error"] = "Sadece moderatör onayı bekleyen başvurular düzenlenebilir.";
            return RedirectToAction(nameof(Detail), new { id = model.Id });
        }

        if (ModelState.IsValid)
        {
            complaint.Title = model.Title;
            complaint.Description = model.Description;
            complaint.Type = model.Type;
            complaint.Category = model.Category;
            complaint.Location = model.Location;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Başvurunuz başarıyla güncellendi.";
            return RedirectToAction(nameof(Detail), new { id = model.Id });
        }

        model.CurrentStatus = complaint.Status;
        model.InstitutionName = complaint.Institution?.Name ?? "";
        return View(model);
    }
}
