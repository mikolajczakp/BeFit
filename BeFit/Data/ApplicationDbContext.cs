using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BeFit.Models;

namespace BeFit.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<ExerciseType> ExerciseTypes => Set<ExerciseType>();

    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();

    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<WorkoutExercise>()
            .HasOne(exercise => exercise.ExerciseType)
            .WithMany()
            .HasForeignKey(exercise => exercise.ExerciseTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
