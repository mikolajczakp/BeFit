using System.ComponentModel.DataAnnotations;

namespace BeFit.Models;

public record ExerciseStatistics(
    [property: Display(Name = "Typ ćwiczenia")] string ExerciseName,
    [property: Display(Name = "Liczba wykonań")] int ExecutionCount,
    [property: Display(Name = "Łączna liczba powtórzeń")] long TotalRepetitions,
    [property: Display(Name = "Średnie obciążenie (kg)")] decimal AverageWeightKg,
    [property: Display(Name = "Maksymalne obciążenie (kg)")] decimal MaximumWeightKg);
