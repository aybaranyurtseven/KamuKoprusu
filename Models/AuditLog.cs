namespace KamuKoprusu.Models;

public class AuditLog
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // "UserCreated", "ComplaintApproved", etc.
    public string EntityType { get; set; } = string.Empty; // "User", "Complaint", etc.
    public int? EntityId { get; set; }
    public string? Details { get; set; } // JSON or text with additional info
    public string IpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public ApplicationUser? User { get; set; }
}
