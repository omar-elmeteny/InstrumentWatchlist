using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Data.DTOs;

public class CreateWatchlistItem : IValidatableObject
{
    [Required(ErrorMessage = "Symbol is required.")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "Symbol must be between 1 and 10 characters.")]
    public string Symbol { get; set; } = string.Empty;

    [Required(ErrorMessage = "Target Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Target Price must be greater than 0.")]
    public decimal TargetPrice { get; set; }

    [StringLength(250, ErrorMessage = "Note cannot exceed 250 characters.")]
    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TargetPrice != decimal.Round(TargetPrice, 2))
        {
            yield return new ValidationResult(
                "Target Price cannot have more than two decimal places.",
                [nameof(TargetPrice)]);
        }
    }
}