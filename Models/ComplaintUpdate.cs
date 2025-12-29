using KamuKoprusu.Enums;

namespace KamuKoprusu.Models;

public class ComplaintUpdate
{
    public int Id { get; set; }
    public int ComplaintId { get; set; }
    public string Message { get; set; } = string.Empty;
    public ComplaintStatus NewStatus { get; set; }
    public string? UpdatedByUserId { get; set; } // Institution rep or moderator
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Complaint Complaint { get; set; } = null!;
    public ApplicationUser? UpdatedBy { get; set; }
    public ICollection<Media> MediaFiles { get; set; } = new List<Media>();
}
