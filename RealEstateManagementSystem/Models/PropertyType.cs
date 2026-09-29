using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.Models
{
    /// <summary>
    /// Lookup table for the kind of property (Apartment, Villa, House, Plot, Office, Shop, Farmhouse...).
    /// The values are stored in the database and are managed by Administrators only,
    /// they are never hard-coded inside controllers.
    /// </summary>
    public class PropertyType
    {
        public int PropertyTypeId { get; set; }

        [Required(ErrorMessage = "Property Type name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Property Type name must be between 2 and 50 characters.")]
        [Display(Name = "Property Type")]
        public string Name { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Properties that currently use this type.
        /// A type with existing properties can never be deleted - it is deactivated instead.
        /// </summary>
        public ICollection<Property> Properties { get; set; } = new List<Property>();
    }
}
