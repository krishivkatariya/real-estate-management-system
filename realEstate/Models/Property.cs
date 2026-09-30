using System.ComponentModel.DataAnnotations;

namespace realEstate.Models;

public class Property
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Area { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(20)]
    public string? ZipCode { get; set; }

    public PropertyType PropertyType { get; set; } = PropertyType.Other;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public PropertyStatus Status { get; set; } = PropertyStatus.Available;

    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }

    // Owner (ApplicationUser) - optional
    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }

    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}
