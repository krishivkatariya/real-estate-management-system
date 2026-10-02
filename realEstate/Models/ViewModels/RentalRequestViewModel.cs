using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace realEstate.Models.ViewModels;

public class RentalRequestViewModel : IValidatableObject
{
    [Required]
    public int PropertyId { get; set; }

    [BindNever]
    public string PropertyTitle { get; set; } = string.Empty;

    [BindNever]
    public decimal MonthlyRent { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.Date < DateTime.UtcNow.Date)
        {
            yield return new ValidationResult("The start date cannot be in the past.", [nameof(StartDate)]);
        }

        if (EndDate.Date <= StartDate.Date)
        {
            yield return new ValidationResult("The end date must be after the start date.", [nameof(EndDate)]);
        }
    }
}