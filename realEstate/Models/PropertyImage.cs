using System.ComponentModel.DataAnnotations;

namespace realEstate.Models;

public class PropertyImage
{
    public int Id { get; set; }

    public int PropertyId { get; set; }
    public Property? Property { get; set; }

    [Required]
    [StringLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string FilePath { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}
