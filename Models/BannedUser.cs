namespace KamuKoprusu.Models;

public class BannedUser
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime BannedAt { get; set; } = DateTime.UtcNow;
    public string BannedByUserId { get; set; } = string.Empty; // Admin or moderator
    public bool IsPermanent { get; set; } = true;
    public DateTime? BanExpiresAt { get; set; }
    public DateTime? UnbannedAt { get; set; }
    
    // Banned credentials - to prevent re-registration
    public string? BannedEmail { get; set; }
    public string? BannedPhone { get; set; }
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser BannedBy { get; set; } = null!;
}
