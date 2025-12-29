using KamuKoprusu.Enums;

namespace KamuKoprusu.Models;

public class Complaint
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ComplaintType Type { get; set; }
    public string Category { get; set; } = string.Empty; // "Technical", "Administrative", "Content", etc.
    public ComplaintStatus Status { get; set; } = ComplaintStatus.PendingModeration;
    public bool IsAnonymous { get; set; } = false;
    public bool IsApproved { get; set; } = false; // Moderator approval
    public string? RejectionReason { get; set; }
    public string? Location { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    
    // Foreign keys
    public string UserId { get; set; } = string.Empty;
    public int InstitutionId { get; set; }
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Institution Institution { get; set; } = null!;
    public ICollection<Media> MediaFiles { get; set; } = new List<Media>();
    public ICollection<ComplaintUpdate> Updates { get; set; } = new List<ComplaintUpdate>();
}
