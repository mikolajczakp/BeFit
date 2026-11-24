using System.ComponentModel.DataAnnotations;

namespace BeFit.Models;

public class ExerciseType
{
    public int Id { get; set; }

    [Display(Name = "Nazwa ćwiczenia")]
    [Required(ErrorMessage = "Podaj nazwę ćwiczenia.")]
    [StringLength(100, ErrorMessage = "Nazwa ćwiczenia może mieć maksymalnie {1} znaków.")]
    public string Name { get; set; } = string.Empty;
}
