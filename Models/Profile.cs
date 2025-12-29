namespace KamuKoprusu.Models;

public class Profile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
}
