using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BeFit.Models;

public class WorkoutSession : IValidatableObject
{
    public int Id { get; set; }

    [Display(Name = "Początek treningu")]
    [Required(ErrorMessage = "Podaj datę i godzinę rozpoczęcia treningu.")]
    public DateTime? StartedAt { get; set; }

    [Display(Name = "Koniec treningu")]
    [Required(ErrorMessage = "Podaj datę i godzinę zakończenia treningu.")]
    public DateTime? EndedAt { get; set; }

    public string UserId { get; set; } = string.Empty;

    [ValidateNever]
    public IdentityUser User { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartedAt is DateTime start && EndedAt is DateTime end && end <= start)
        {
            yield return new ValidationResult(
                "Koniec treningu musi wypadać po jego rozpoczęciu.",
                [nameof(EndedAt)]);
        }
    }
}
