using System.ComponentModel.DataAnnotations;

namespace realEstate.Models;

public class Agent
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(50)]
    public string? Phone { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
