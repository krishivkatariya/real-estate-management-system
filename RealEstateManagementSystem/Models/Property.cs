using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagementSystem.Models
{
    /// <summary>
    /// A property listing created by an authenticated user.
    /// The same <see cref="ApplicationUser"/> can own many properties (owner) and can later
    /// buy or rent another user's property, so no separate Seller or Buyer entity is required.
    /// </summary>
    public class Property
    {
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 150 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(typeof(decimal), "1", "999999999999", ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Listing type is required.")]
        [Display(Name = "Listing Type")]
        public ListingType ListingType { get; set; }

        [Required(ErrorMessage = "Property Type is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Property Type.")]
        [Display(Name = "Property Type")]
        public int PropertyTypeId { get; set; }

        public PropertyType? PropertyType { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Location.")]
        [Display(Name = "Location")]
        public int LocationId { get; set; }

        public Location? Location { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 300 characters.")]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area is required.")]
        [Range(typeof(decimal), "1", "10000000", ErrorMessage = "Area must be greater than 0.")]
        [Display(Name = "Area (sq. ft.)")]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "Bedrooms is required.")]
        [Range(0, 100, ErrorMessage = "Bedrooms must be 0 or more.")]
        [Display(Name = "Bedrooms")]
        public int Bedrooms { get; set; }

        [Required(ErrorMessage = "Bathrooms is required.")]
        [Range(0, 100, ErrorMessage = "Bathrooms must be 0 or more.")]
        [Display(Name = "Bathrooms")]
        public int Bathrooms { get; set; }

        /// <summary>
        /// Approval/workflow status. Newly created listings always start as <see cref="PropertyStatus.Pending"/>
        /// and can only be changed by an Administrator.
        /// </summary>
        [Display(Name = "Status")]
        public PropertyStatus Status { get; set; } = PropertyStatus.Pending;

        /// <summary>Optional reason captured by an Administrator when a listing is rejected.</summary>
        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters.")]
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        /// <summary>Foreign key to the owning <see cref="ApplicationUser"/>. Never taken from the browser.</summary>
        [Required]
        public string OwnerId { get; set; } = string.Empty;

        public ApplicationUser? Owner { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Images uploaded for this property (one folder on disk, paths stored in the database).</summary>
        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();

        /// <summary>
        /// Primary image of the listing, or the first available image, or null.
        /// Not mapped: it is only a convenience helper for the Razor views.
        /// </summary>
        [NotMapped]
        public PropertyImage? PrimaryImage =>
            Images.FirstOrDefault(i => i.IsPrimary) ?? Images.OrderBy(i => i.PropertyImageId).FirstOrDefault();
    }
}
