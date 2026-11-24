using BeFit.Data;
using BeFit.Models;
using BeFit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Controllers;

public class ExerciseTypesController(ApplicationDbContext db) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exerciseTypes = await db.ExerciseTypes
            .AsNoTracking()
            .OrderBy(exerciseType => exerciseType.Name)
            .ToListAsync(cancellationToken);

        return View(exerciseTypes);
    }

    [HttpGet]
    [Authorize(Roles = Administrator.Role)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = Administrator.Role)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(nameof(ExerciseType.Name))] ExerciseType exerciseType,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(exerciseType);
        }

        exerciseType.Name = exerciseType.Name.Trim();
        db.ExerciseTypes.Add(exerciseType);
        await db.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = Administrator.Role)]
    public async Task<IActionResult> Edit([FromRoute] int id, CancellationToken cancellationToken)
    {
        var exerciseType = await db.ExerciseTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return exerciseType is null ? NotFound() : View(exerciseType);
    }

    [HttpPost]
    [Authorize(Roles = Administrator.Role)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        [FromRoute] int id,
        [Bind(nameof(ExerciseType.Name))] ExerciseType exerciseType,
        CancellationToken cancellationToken)
    {
        var matchingExercise = db.ExerciseTypes.Where(item => item.Id == id);

        if (!ModelState.IsValid)
        {
            if (!await matchingExercise.AnyAsync(cancellationToken))
            {
                return NotFound();
            }

            exerciseType.Id = id;
            return View(exerciseType);
        }

        var name = exerciseType.Name.Trim();
        var updatedRows = await matchingExercise.ExecuteUpdateAsync(
            setters => setters.SetProperty(item => item.Name, name),
            cancellationToken);

        return updatedRows == 0 ? NotFound() : RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = Administrator.Role)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var exerciseType = await db.ExerciseTypes
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return exerciseType is null ? NotFound() : View(exerciseType);
    }

    [HttpPost, ActionName(nameof(Delete))]
    [Authorize(Roles = Administrator.Role)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id, CancellationToken cancellationToken)
    {
        var matchingType = db.ExerciseTypes.Where(item => item.Id == id);
        var deletedRows = await matchingType
            .Where(item => !db.WorkoutExercises.Any(exercise => exercise.ExerciseTypeId == item.Id))
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows != 0)
        {
            return RedirectToAction(nameof(Index));
        }

        var exerciseType = await matchingType.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        if (exerciseType is null)
        {
            return NotFound();
        }

        ModelState.AddModelError(string.Empty, "Tego typu ćwiczenia nie można usunąć, ponieważ jest używany w treningach.");
        return View(nameof(Delete), exerciseType);
    }
}
