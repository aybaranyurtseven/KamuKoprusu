using System.ComponentModel.DataAnnotations;

namespace KamuKoprusu.Models;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Ad Soyad gereklidir")]
    [Display(Name = "Ad Soyad")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Telefon Numarası")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Hakkımda")]
    [StringLength(500)]
    public string? Bio { get; set; }

    [Display(Name = "Şehir")]
    [StringLength(100)]
    public string? City { get; set; }

    [Display(Name = "İlçe")]
    [StringLength(100)]
    public string? District { get; set; }

    [Display(Name = "Twitter")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    public string? TwitterUrl { get; set; }

    [Display(Name = "LinkedIn")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    public string? LinkedInUrl { get; set; }

    [Display(Name = "Instagram")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz")]
    public string? InstagramUrl { get; set; }
}
