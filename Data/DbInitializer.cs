using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using KamuKoprusu.Models;

namespace KamuKoprusu.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed default admin user
        await SeedAdminUserAsync(userManager);

        // Seed badges
        await SeedBadgesAsync(context);

        // Seed sample institutions
        await SeedInstitutionsAsync(context);

        // Seed institution representative users
        await SeedInstitutionUsersAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Citizen", "InstitutionRepresentative", "Moderator", "Admin" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@kamukoprusu.com";
        
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Sistem Admin",
                EmailConfirmed = true,
                IsApproved = true,
                IsBanned = false
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedBadgesAsync(ApplicationDbContext context)
    {
        if (await context.Badges.AnyAsync())
        {
            return; // Badges already seeded
        }

        var badges = new List<Badge>
        {
            new Badge
            {
                Name = "Söz Sahibi",
                Description = "İlk başvurunuzu gönderdiniz",
                IconUrl = "/images/badges/first-submission.png",
                IconClass = "bi-chat-left-text",
                RequiredCount = 1,
                CriteriaType = "ComplaintSubmitted"
            },
            new Badge
            {
                Name = "Fark Yaratıcı",
                Description = "3 başvuru gönderdiniz",
                IconUrl = "/images/badges/difference-maker.png",
                IconClass = "bi-megaphone",
                RequiredCount = 3,
                CriteriaType = "ComplaintSubmitted"
            },
            new Badge
            {
                Name = "Sürekli Çaba",
                Description = "10 başvuru gönderdiniz",
                IconUrl = "/images/badges/persistent.png",
                IconClass = "bi-fire",
                RequiredCount = 10,
                CriteriaType = "ComplaintSubmitted"
            },
            new Badge
            {
                Name = "Değişim Habercisi",
                Description = "25 başvuru gönderdiniz",
                IconUrl = "/images/badges/change-herald.png",
                IconClass = "bi-broadcast",
                RequiredCount = 25,
                CriteriaType = "ComplaintSubmitted"
            },
            new Badge
            {
                Name = "Toplum Savunucusu",
                Description = "50 başvuru gönderdiniz",
                IconUrl = "/images/badges/community-defender.png",
                IconClass = "bi-shield-check",
                RequiredCount = 50,
                CriteriaType = "ComplaintSubmitted"
            },
            new Badge
            {
                Name = "İçerik Ustası",
                Description = "5 medya dosyalı başvuru gönderdiniz",
                IconUrl = "/images/badges/content-master.png",
                IconClass = "bi-camera-video",
                RequiredCount = 5,
                CriteriaType = "MediaUploaded"
            },
            new Badge
            {
                Name = "Hızlı Çözüm",
                Description = "3 gün içinde çözülen başvurunuz var",
                IconUrl = "/images/badges/quick-resolution.png",
                IconClass = "bi-lightning",
                RequiredCount = 1,
                CriteriaType = "QuickResolution"
            },
            new Badge
            {
                Name = "Başarı Hikayesi",
                Description = "İlk başvurunuz çözüldü",
                IconUrl = "/images/badges/success-story.png",
                IconClass = "bi-trophy",
                RequiredCount = 1,
                CriteriaType = "ComplaintResolved"
            }
        };

        await context.Badges.AddRangeAsync(badges);
        await context.SaveChangesAsync();
    }

    private static async Task SeedInstitutionsAsync(ApplicationDbContext context)
    {
        if (await context.Institutions.AnyAsync())
        {
            return; // Institutions already seeded
        }

        var institutions = new List<Institution>
        {
            new Institution
            {
                Name = "Sağlık Bakanlığı",
                Type = "Bakanlık",
                InstitutionCode = "SB-001",
                Email = "iletisim@saglik.gov.tr",
                Phone = "0312 XXX XX XX",
                About = "Türkiye Cumhuriyeti Sağlık Bakanlığı",
                LogoUrl = "/images/logo.png"
            },
            new Institution
            {
                Name = "Milli Eğitim Bakanlığı",
                Type = "Bakanlık",
                InstitutionCode = "MEB-001",
                Email = "iletisim@meb.gov.tr",
                Phone = "0312 XXX XX XX",
                About = "Türkiye Cumhuriyeti Milli Eğitim Bakanlığı",
                LogoUrl = "/images/logo.png"
            },
            new Institution
            {
                Name = "Ulaştırma Bakanlığı",
                Type = "Bakanlık",
                InstitutionCode = "UB-001",
                Email = "iletisim@uab.gov.tr",
                Phone = "0312 XXX XX XX",
                About = "Türkiye Cumhuriyeti Ulaştırma ve Altyapı Bakanlığı",
                LogoUrl = "/images/logo.png"
            },
            new Institution
            {
                Name = "Çevre, Şehircilik ve İklim Değişikliği Bakanlığı",
                Type = "Bakanlık",
                InstitutionCode = "CSB-001",
                Email = "iletisim@csb.gov.tr",
                Phone = "0312 XXX XX XX",
                About = "Türkiye Cumhuriyeti Çevre, Şehircilik ve İklim Değişikliği Bakanlığı",
                LogoUrl = "/images/logo.png"
            },
            new Institution
            {
                Name = "İçişleri Bakanlığı",
                Type = "Bakanlık",
                InstitutionCode = "ICB-001",
                Email = "iletisim@icisleri.gov.tr",
                Phone = "0312 XXX XX XX",
                About = "Türkiye Cumhuriyeti İçişleri Bakanlığı",
                LogoUrl = "/images/logo.png"
            }
        };

        await context.Institutions.AddRangeAsync(institutions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedInstitutionUsersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Institution users with email format: kurum@kurum.gov.tr
        var institutionUsers = new[]
        {
            new { Email = "saglik@saglik.gov.tr", FullName = "Sağlık Bakanlığı Temsilcisi", InstitutionCode = "SB-001" },
            new { Email = "egitim@meb.gov.tr", FullName = "MEB Temsilcisi", InstitutionCode = "MEB-001" },
            new { Email = "ulastirma@uab.gov.tr", FullName = "Ulaştırma Bakanlığı Temsilcisi", InstitutionCode = "UB-001" },
            new { Email = "cevre@cevre.gov.tr", FullName = "Çevre Bakanlığı Temsilcisi", InstitutionCode = "CSB-001" },
            new { Email = "icisleri@icisleri.gov.tr", FullName = "İçişleri Bakanlığı Temsilcisi", InstitutionCode = "ICB-001" }
        };

        foreach (var userData in institutionUsers)
        {
            if (await userManager.FindByEmailAsync(userData.Email) != null)
                continue;

            var institution = await context.Institutions
                .FirstOrDefaultAsync(i => i.InstitutionCode == userData.InstitutionCode);

            if (institution == null)
                continue;

            var user = new ApplicationUser
            {
                UserName = userData.Email,
                Email = userData.Email,
                FullName = userData.FullName,
                EmailConfirmed = true,
                IsApproved = true,
                IsBanned = false,
                InstitutionId = institution.Id
            };

            var result = await userManager.CreateAsync(user, "Admin123!");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "InstitutionRepresentative");
            }
        }
    }
}
