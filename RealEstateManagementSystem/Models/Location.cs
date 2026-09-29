using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagementSystem.Models
{
    /// <summary>
    /// Lookup table for the place where a property is located.
    /// The same location can be re-used by many properties.
    /// Locations are managed by Administrators and are read from the database by the property forms.
    /// </summary>
    public class Location
    {
        public int LocationId { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "City must be between 2 and 50 characters.")]
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

        /// <summary>Properties that belong to this location.</summary>
        public ICollection<Property> Properties { get; set; } = new List<Property>();

        /// <summary>Readable label used by dropdowns and screens.</summary>
        [NotMapped]
        [Display(Name = "Location")]
        public string DisplayName => $"{City}, {State} - {Pincode}";
    }
}
