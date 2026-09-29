using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.Models.ViewModels
{
    /// <summary>Backing model for the Admin rejection form (/Admin/Properties/Reject/{id}).</summary>
    public class PropertyRejectViewModel
    {
        public int PropertyId { get; set; }

        [Display(Name = "Property")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide a reason for rejecting this listing.")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Rejection reason must be between 5 and 500 characters.")]
        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
