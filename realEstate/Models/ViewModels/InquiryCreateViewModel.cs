using System.ComponentModel.DataAnnotations;

namespace realEstate.Models.ViewModels
{
    public class InquiryCreateViewModel
    {
        public int PropertyId { get; set; }

        public string PropertyTitle { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = null!;
    }
}
