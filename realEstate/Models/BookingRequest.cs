using System;
using System.ComponentModel.DataAnnotations;

namespace realEstate.Models
{
    public enum BookingStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }

    public class BookingRequest
    {
        public int Id { get; set; }

        [Required]
        public string BuyerId { get; set; } = null!;

        [Required]
        public int PropertyId { get; set; }

        [Required]
        public string SellerId { get; set; } = null!;

        [Required]
        public DateTime PreferredDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PreferredTime { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Message { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public string? SellerResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        // Navigation
        public ApplicationUser? Buyer { get; set; }
        public ApplicationUser? Seller { get; set; }
        public Property? Property { get; set; }
    }
}
