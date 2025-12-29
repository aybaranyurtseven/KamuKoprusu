namespace KamuKoprusu.Models;

public class Institution
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "Ministry", "Municipality", etc.
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? About { get; set; }
    public string? LogoUrl { get; set; }
    public string InstitutionCode { get; set; } = string.Empty; // Unique identifier
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
