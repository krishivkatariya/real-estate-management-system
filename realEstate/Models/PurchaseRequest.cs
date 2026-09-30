using System.ComponentModel.DataAnnotations;

namespace realEstate.Models;

public class PurchaseRequest
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty; // buyer
    public ApplicationUser? User { get; set; }

    [Required]
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    [StringLength(2000)]
    public string? Message { get; set; }

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Pending;
}
