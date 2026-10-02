using System.ComponentModel.DataAnnotations;

namespace realEstate.Models;

public class RentalTransaction
{
    public int RentalTransactionId { get; set; }

    [Required]
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    [Required]
    public string RenterId { get; set; } = string.Empty;
    public ApplicationUser? Renter { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal RentAmount { get; set; }

    public RentalStatus Status { get; set; } = RentalStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}