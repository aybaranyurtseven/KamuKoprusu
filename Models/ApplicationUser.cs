using Microsoft.AspNetCore.Identity;
using KamuKoprusu.Enums;

namespace KamuKoprusu.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public int ReputationScore { get; set; } = 0;
    public UserLevel Level { get; set; } = UserLevel.Bronze;
    public bool IsApproved { get; set; } = true; // For institution reps, requires admin approval
    public bool IsBanned { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // For Institution Representatives
    public int? InstitutionId { get; set; }
    public Institution? Institution { get; set; }
    
    // Navigation properties
    public Profile? Profile { get; set; }
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    public ICollection<Warning> Warnings { get; set; } = new List<Warning>();
}
