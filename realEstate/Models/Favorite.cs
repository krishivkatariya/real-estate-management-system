using System.ComponentModel.DataAnnotations;

namespace realEstate.Models;

public class Favorite
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required]
    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
