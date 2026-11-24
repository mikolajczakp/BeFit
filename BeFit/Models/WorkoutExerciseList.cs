namespace BeFit.Models;

public record WorkoutExerciseList(WorkoutSession Session, IReadOnlyList<WorkoutExercise> Exercises);
