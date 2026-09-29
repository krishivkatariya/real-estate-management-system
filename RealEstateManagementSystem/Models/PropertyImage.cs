using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagementSystem.Models
{
    /// <summary>
    /// Stores the URL (a relative path under wwwroot) of a single property image.
    /// The physical file lives in wwwroot/uploads/properties and is NEVER stored as binary in SQL Server.
    /// Relationship: Property 1 ---- many PropertyImage.
    /// </summary>
    public class PropertyImage
    {
        public int PropertyImageId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        /// <summary>Relative web path of the uploaded file, e.g. "/uploads/properties/&lt;guid&gt;.jpg".</summary>
        [Required(ErrorMessage = "Image path is required.")]
        [StringLength(300)]
        [Display(Name = "Image")]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>True for the single image that represents the listing in cards and galleries.</summary>
        [Display(Name = "Primary Image")]
        public bool IsPrimary { get; set; }

        public Property? Property { get; set; }
    }
}
