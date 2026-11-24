using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BeFit.Models;

public class WorkoutExercise
{
    public int Id { get; set; }

    [Display(Name = "Sesja treningowa")]
    [Range(1, int.MaxValue, ErrorMessage = "Wybierz sesję treningową.")]
    public int WorkoutSessionId { get; set; }

    [ValidateNever]
    public WorkoutSession WorkoutSession { get; set; } = null!;

    [Display(Name = "Typ ćwiczenia")]
    [Range(1, int.MaxValue, ErrorMessage = "Wybierz typ ćwiczenia.")]
    public int ExerciseTypeId { get; set; }

    [ValidateNever]
    public ExerciseType ExerciseType { get; set; } = null!;

    [Display(Name = "Obciążenie (kg)")]
    [Required(ErrorMessage = "Podaj obciążenie. Wpisz 0, jeśli ćwiczysz bez dodatkowego ciężaru.")]
    [Range(typeof(decimal), "0", "1000", ErrorMessage = "Obciążenie musi mieścić się w zakresie od {1} do {2} kg.")]
    public decimal? WeightKg { get; set; }

    [Display(Name = "Liczba serii")]
    [Required(ErrorMessage = "Podaj liczbę serii.")]
    [Range(1, 100, ErrorMessage = "Liczba serii musi mieścić się w zakresie od {1} do {2}.")]
    public int? Sets { get; set; }

    [Display(Name = "Powtórzenia w serii")]
    [Required(ErrorMessage = "Podaj liczbę powtórzeń w serii.")]
    [Range(1, 1000, ErrorMessage = "Liczba powtórzeń w serii musi mieścić się w zakresie od {1} do {2}.")]
    public int? RepetitionsPerSet { get; set; }
}
