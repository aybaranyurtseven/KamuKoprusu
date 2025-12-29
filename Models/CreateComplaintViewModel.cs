using System.ComponentModel.DataAnnotations;
using KamuKoprusu.Enums;

namespace KamuKoprusu.Models;

public class CreateComplaintViewModel
{
    [Required(ErrorMessage = "Başlık gereklidir")]
    [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama gereklidir")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Açıklama 20-5000 karakter arasında olmalıdır")]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şikayet türü seçiniz")]
    [Display(Name = "Tür")]
    public ComplaintType Type { get; set; }

    [Required(ErrorMessage = "Kategori gereklidir")]
    [StringLength(100)]
    [Display(Name = "Kategori")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kurum seçiniz")]
    [Display(Name = "İlgili Kurum")]
    public int InstitutionId { get; set; }

    [Display(Name = "Konum")]
    [StringLength(500)]
    public string? Location { get; set; }

    [Display(Name = "Enlem")]
    public double? Latitude { get; set; }

    [Display(Name = "Boylam")]
    public double? Longitude { get; set; }

    [Display(Name = "Anonim Gönder")]
    public bool IsAnonymous { get; set; }
}
