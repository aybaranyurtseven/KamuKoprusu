namespace KamuKoprusu.Models;

public class Badge
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string IconClass { get; set; } = "bi-award"; // Bootstrap icon class
    public int RequiredCount { get; set; } // How many actions needed to earn
    public string CriteriaType { get; set; } = string.Empty; // "ComplaintSubmitted", "ComplaintResolved", etc.
    
    // Navigation properties
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
}
