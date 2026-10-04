using System.ComponentModel.DataAnnotations;

namespace realEstate.Models.AccountViewModels;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Account type")]
    [RegularExpression("Buyer|Seller|Admin", ErrorMessage = "Invalid role selection.")]
    public string Role { get; set; } = "Buyer";
}
