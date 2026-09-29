using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.Models.ViewModels
{
    /// <summary>Backing model for the Admin Property Type create/edit screens.</summary>
    public class PropertyTypeFormViewModel
    {
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "Property Type name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Name cannot contain only whitespace.")]
        [Display(Name = "Property Type Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}
