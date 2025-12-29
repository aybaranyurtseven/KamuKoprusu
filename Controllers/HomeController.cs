using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Models;
using KamuKoprusu.Data;
using KamuKoprusu.Enums;

namespace KamuKoprusu.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Public statistics
        var totalComplaints = await _context.Complaints.CountAsync();
        var resolvedComplaints = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Resolved);
        var totalUsers = await _context.Users.CountAsync(u => !u.IsBanned);
        var totalInstitutions = await _context.Institutions.CountAsync();

        // Resolution rate
        var resolutionRate = totalComplaints > 0 ? (resolvedComplaints * 100 / totalComplaints) : 0;

        // Recent resolved complaints (public, non-anonymous)
        var recentResolved = await _context.Complaints
            .Where(c => c.Status == ComplaintStatus.Resolved && !c.IsAnonymous && c.IsApproved)
            .Include(c => c.Institution)
            .OrderByDescending(c => c.ResolvedAt)
            .Take(5)
            .ToListAsync();

        // Category statistics
        var categoryStats = await _context.Complaints
            .Where(c => c.IsApproved)
            .GroupBy(c => c.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        // Institution performance (top 5 by resolution rate)
        var institutionStats = await _context.Complaints
            .Where(c => c.IsApproved)
            .GroupBy(c => c.Institution)
            .Select(g => new
            {
                Institution = g.Key!.Name,
                Total = g.Count(),
                Resolved = g.Count(c => c.Status == ComplaintStatus.Resolved)
            })
            .Where(x => x.Total >= 5) // At least 5 complaints
            .OrderByDescending(x => x.Resolved * 100 / x.Total)
            .Take(5)
            .ToListAsync();

        ViewBag.Stats = new
        {
            TotalComplaints = totalComplaints,
            ResolvedComplaints = resolvedComplaints,
            TotalUsers = totalUsers,
            TotalInstitutions = totalInstitutions,
            ResolutionRate = resolutionRate
        };

        ViewBag.RecentResolved = recentResolved;
        ViewBag.CategoryStats = categoryStats;
        ViewBag.InstitutionStats = institutionStats;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
