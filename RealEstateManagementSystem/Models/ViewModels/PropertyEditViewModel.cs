using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RealEstateManagementSystem.Models.ViewModels
{
    /// <summary>
    /// Backing model for /Properties/Edit/{id}.
    /// OwnerId, CreatedAt, Status and RejectionReason are intentionally absent - a normal user
    /// can never change them through the form.
    /// </summary>
    public class PropertyEditViewModel
    {
        public int PropertyId { get; set; }

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

        /// <summary>New images to append to the listing.</summary>
        [Display(Name = "Add New Images")]
        public List<IFormFile>? Images { get; set; }

        /// <summary>Ids of the existing images the owner ticked for removal (read-only list to the user).</summary>
        public List<int> RemoveImageIds { get; set; } = new List<int>();

        /// <summary>Existing image that should become the primary image.</summary>
        [Display(Name = "Primary Image")]
        public int? PrimaryImageId { get; set; }

        /// <summary>Read-only info about the listing being edited.</summary>
        public PropertyStatus CurrentStatus { get; set; }

        public string? RejectionReason { get; set; }

        public string? OwnerName { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>Existing images of the property, filled by the controller.</summary>
        [BindNever]
        public List<PropertyImage> ExistingImages { get; set; } = new List<PropertyImage>();

        [BindNever]
        public IEnumerable<SelectListItem> PropertyTypes { get; set; } = new List<SelectListItem>();

        [BindNever]
        public IEnumerable<SelectListItem> Locations { get; set; } = new List<SelectListItem>();
    }
}
