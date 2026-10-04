using System;
using System.ComponentModel.DataAnnotations;

namespace realEstate.Models
{
    public enum InquiryStatus
    {
        Pending = 0,
        InProgress = 1,
        Responded = 2,
        Closed = 3
    }

    public class Inquiry
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public int PropertyId { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = null!;

        public InquiryStatus Status { get; set; } = InquiryStatus.Pending;

        public string? ResponseMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        // Navigation properties
        public ApplicationUser? User { get; set; }
        public Property? Property { get; set; }
    }
}
