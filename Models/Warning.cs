namespace KamuKoprusu.Models;

public class Warning
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int? ComplaintId { get; set; } // Optional reference to the offending complaint
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string IssuedByUserId { get; set; } = string.Empty; // Moderator who issued warning
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Complaint? Complaint { get; set; }
    public ApplicationUser IssuedBy { get; set; } = null!;
}
