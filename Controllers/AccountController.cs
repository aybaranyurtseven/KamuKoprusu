using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Models;
using KamuKoprusu.Enums;
using KamuKoprusu.Data;

namespace KamuKoprusu.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        // Load institutions for dropdown
        ViewBag.Institutions = await _context.Institutions.OrderBy(i => i.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            Institution? institution = null;
            
            // For institution representatives, find and verify the institution
            if (model.Role == "InstitutionRepresentative")
            {
                if (!model.InstitutionId.HasValue)
                {
                    ModelState.AddModelError("InstitutionId", "Lütfen kurumunuzu seçin.");
                    
                    // Load institutions for re-display
                    ViewBag.Institutions = await _context.Institutions.OrderBy(i => i.Name).ToListAsync();
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.InstitutionCode))
                {
                    ModelState.AddModelError("InstitutionCode", "Kurum kimlik kodu gereklidir.");
                    
                    // Load institutions for re-display
                    ViewBag.Institutions = await _context.Institutions.OrderBy(i => i.Name).ToListAsync();
                    return View(model);
                }

                // Find institution by ID and verify with code
                institution = await _context.Institutions
                    .FirstOrDefaultAsync(i => i.Id == model.InstitutionId.Value && i.InstitutionCode == model.InstitutionCode);

                if (institution == null)
                {
                    ModelState.AddModelError("InstitutionCode", "Geçersiz kurum kimlik kodu. Lütfen kurumunuzun doğru kimlik kodunu girin.");
                    
                    // Load institutions for re-display
                    ViewBag.Institutions = await _context.Institutions.OrderBy(i => i.Name).ToListAsync();
                    return View(model);
                }
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                IsApproved = model.Role != "InstitutionRepresentative", // Institution reps need approval
                InstitutionId = institution?.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add role to user
                await _userManager.AddToRoleAsync(user, model.Role);

                // Create profile
                var profile = new Profile
                {
                    UserId = user.Id
                };
                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();

                // If institution representative, handle special setup
                if (model.Role == "InstitutionRepresentative" && !string.IsNullOrEmpty(model.InstitutionCode))
                {
                    // Mark as needing approval
                    user.IsApproved = false;
                    await _userManager.UpdateAsync(user);
                    
                    TempData["Message"] = "Başvurunuz alındı. Admin onayından sonra giriş yapabilirsiniz.";
                    return RedirectToAction("Login");
                }

                // Auto sign in for citizens
                if (model.Role == "Citizen")
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Dashboard", "Citizen");
                }

                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                // Skip empty error messages (like duplicate username when we only use email)
                if (!string.IsNullOrWhiteSpace(error.Description))
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                return View(model);
            }

            // Check if user is approved
            if (!user.IsApproved)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız henüz onaylanmamış.");
                return View(model);
            }

            // Check if user is banned
            if (user.IsBanned)
            {
                ModelState.AddModelError(string.Empty, "Hesabınız yasaklanmış.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                // Redirect based on role
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (roles.Contains("Moderator"))
                {
                    return RedirectToAction("Dashboard", "Moderator");
                }
                else if (roles.Contains("InstitutionRepresentative"))
                {
                    return RedirectToAction("Dashboard", "Institution");
                }
                else // Citizen
                {
                    return RedirectToAction("Dashboard", "Citizen");
                }
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "Hesabınız 10 dakika boyunca kilitlendi.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
