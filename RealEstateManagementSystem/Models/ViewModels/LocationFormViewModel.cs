using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.Models.ViewModels
{
    /// <summary>Backing model for the Admin Location create/edit screens.</summary>
    public class LocationFormViewModel
    {
        public int LocationId { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "City must be between 2 and 50 characters.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "City cannot contain only whitespace.")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "State must be between 2 and 50 characters.")]
        [Display(Name = "State")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Country must be between 2 and 50 characters.")]
        [Display(Name = "Country")]
        public string Country { get; set; } = "India";

        [Required(ErrorMessage = "Pincode is required.")]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Please enter a valid 6-digit pincode.")]
        [Display(Name = "Pincode")]
        public string Pincode { get; set; } = string.Empty;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
