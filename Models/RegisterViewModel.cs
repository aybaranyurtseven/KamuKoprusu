using System.ComponentModel.DataAnnotations;
using KamuKoprusu.Validators;

namespace KamuKoprusu.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad Soyad gereklidir")]
    [Display(Name = "Ad Soyad")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gereklidir")]
    [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrar gereklidir")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [TurkishPhoneNumber]
    [Display(Name = "Telefon Numarası")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Rol seçimi gereklidir")]
    [Display(Name = "Kullanıcı Rolü")]
    public string Role { get; set; } = "Citizen";

    // Institution Representative fields
    [Display(Name = "Kurum")]
    public int? InstitutionId { get; set; }
    
    [Display(Name = "Kurum Kimlik Kodu")]
    [StringLength(50)]
    public string? InstitutionCode { get; set; }
}
