using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [Display(Name = "Email")]
        public required string Email { get; set; }
    }
}
