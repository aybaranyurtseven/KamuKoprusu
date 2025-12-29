using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace KamuKoprusu.Validators;

public class TurkishPhoneNumberAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            // Phone number is optional, so null/empty is valid
            return ValidationResult.Success;
        }

        var phoneNumber = value.ToString()!;
        
        // Remove spaces, dashes, and parentheses
        phoneNumber = Regex.Replace(phoneNumber, @"[\s\-\(\)]", "");
        
        // Check if it starts with 05 and has exactly 11 digits
        if (!Regex.IsMatch(phoneNumber, @"^05\d{9}$"))
        {
            return new ValidationResult("Telefon numarası 05 ile başlamalı ve toplam 11 rakamdan oluşmalıdır. (Örn: 05xxxxxxxxx)");
        }

        return ValidationResult.Success;
    }
}
