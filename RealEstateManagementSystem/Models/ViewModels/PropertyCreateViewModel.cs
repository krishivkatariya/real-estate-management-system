using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RealEstateManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Backing model for /Properties/Create.
    /// Deliberately contains NO OwnerId, NO Status and NO CreatedAt: those are decided
    /// server side from the logged-in user so the browser can never override them.
    /// </summary>
    public class PropertyCreateViewModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 150 characters.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Title cannot contain only whitespace.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters.")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(typeof(decimal), "1", "999999999999", ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Listing type is required.")]
        [Display(Name = "Listing Type")]
        public ListingType? ListingType { get; set; }

        [Required(ErrorMessage = "Property Type is required.")]
        [Display(Name = "Property Type")]
        public int? PropertyTypeId { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [Display(Name = "Location")]
        public int? LocationId { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 300 characters.")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area is required.")]
        [Range(typeof(decimal), "1", "10000000", ErrorMessage = "Area must be greater than 0.")]
        [Display(Name = "Area (sq. ft.)")]
        public decimal? Area { get; set; }

        [Required(ErrorMessage = "Bedrooms is required.")]
        [Range(0, 100, ErrorMessage = "Bedrooms must be greater than or equal to 0.")]
        [Display(Name = "Bedrooms")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "Bathrooms is required.")]
        [Range(0, 100, ErrorMessage = "Bathrooms must be greater than or equal to 0.")]
        [Display(Name = "Bathrooms")]
        public int Bathrooms { get; set; }

        [Display(Name = "Property Images")]
        public List<IFormFile>? Images { get; set; }

        /// <summary>Dropdown source filled by the controller, never bound from the request.</summary>
        [BindNever]
        public IEnumerable<SelectListItem> PropertyTypes { get; set; } = new List<SelectListItem>();

        /// <summary>Dropdown source filled by the controller, never bound from the request.</summary>
        [BindNever]
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
    }
}
