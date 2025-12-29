using KamuKoprusu.Enums;

namespace KamuKoprusu.Models;

public class Media
{
    public int Id { get; set; }
    public MediaType Type { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty; // Local storage path
    public string? ThumbnailPath { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign keys (one of these will be null)
    public int? ComplaintId { get; set; }
    public int? ComplaintUpdateId { get; set; }
    
    // Navigation properties
    public Complaint? Complaint { get; set; }
    public ComplaintUpdate? ComplaintUpdate { get; set; }
}
