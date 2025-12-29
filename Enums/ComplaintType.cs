using System.ComponentModel.DataAnnotations;

namespace KamuKoprusu.Enums;

public enum ComplaintType
{
    [Display(Name = "Suç ve Asayiş")]
    Crime = 1,
    
    [Display(Name = "Din İşleri")]
    Religion = 2,
    
    [Display(Name = "Sağlık")]
    Health = 3,
    
    [Display(Name = "Eğitim")]
    Education = 4,
    
    [Display(Name = "Ulaşım")]
    Transportation = 5,
    
    [Display(Name = "Altyapı")]
    Infrastructure = 6,
    
    [Display(Name = "Çevre")]
    Environment = 7,
    
    [Display(Name = "Sosyal Hizmetler")]
    SocialServices = 8,
    
    [Display(Name = "Diğer")]
    Other = 99
}
