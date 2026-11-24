using System.Globalization;
using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Controllers;

[Authorize]
public class WorkoutExercisesController(ApplicationDbContext db, UserManager<IdentityUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] int sessionId, CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var session = await db.WorkoutSessions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == sessionId && item.UserId == userId, cancellationToken);
        if (session is null)
        {
            return NotFound();
        }

        var exercises = await db.WorkoutExercises.AsNoTracking()
            .Where(item => item.WorkoutSessionId == sessionId && item.WorkoutSession.UserId == userId)
            .Include(item => item.ExerciseType)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

        return View(new WorkoutExerciseList(session, exercises));
    }

    [HttpGet]
    public async Task<IActionResult> Create([FromQuery] int sessionId, CancellationToken cancellationToken)
    {
        if (!await OwnsSessionAsync(sessionId, cancellationToken))
        {
            return NotFound();
        }

        return await ShowFormAsync(nameof(Create), new WorkoutExercise { WorkoutSessionId = sessionId }, cancellationToken);
    }

    [HttpPost, ActionName(nameof(Create))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirmed([FromQuery] int sessionId, CancellationToken cancellationToken)
    {
        if (!await OwnsSessionAsync(sessionId, cancellationToken))
        {
            return NotFound();
        }

        var exercise = new WorkoutExercise { WorkoutSessionId = sessionId };
        if (!await ReadExerciseFormAsync(exercise, cancellationToken))
        {
            return await ShowFormAsync(nameof(Create), exercise, cancellationToken);
        }

        db.WorkoutExercises.Add(exercise);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (!await OwnsSessionAsync(sessionId, cancellationToken))
            {
                return NotFound();
            }

            if (await db.ExerciseTypes.AnyAsync(item => item.Id == exercise.ExerciseTypeId, cancellationToken))
            {
                throw;
            }

            ModelState.AddModelError(nameof(WorkoutExercise.ExerciseTypeId), "Wybrany typ ćwiczenia został usunięty. Wybierz inny.");
            return await ShowFormAsync(nameof(Create), exercise, cancellationToken);
        }

        return RedirectToAction(nameof(Index), new { sessionId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit([FromRoute] int id, CancellationToken cancellationToken)
    {
        var exercise = await OwnedExercises(id).AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        return exercise is null ? NotFound() : await ShowFormAsync(nameof(Edit), exercise, cancellationToken);
    }

    [HttpPost, ActionName(nameof(Edit))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirmed([FromRoute] int id, CancellationToken cancellationToken)
    {
        var exercise = await OwnedExercises(id).SingleOrDefaultAsync(cancellationToken);
        if (exercise is null)
        {
            return NotFound();
        }

        var form = new WorkoutExercise { Id = id, WorkoutSessionId = exercise.WorkoutSessionId };
        if (!await ReadExerciseFormAsync(form, cancellationToken))
        {
            return await ShowFormAsync(nameof(Edit), form, cancellationToken);
        }

        exercise.ExerciseTypeId = form.ExerciseTypeId;
        exercise.WeightKg = form.WeightKg;
        exercise.Sets = form.Sets;
        exercise.RepetitionsPerSet = form.RepetitionsPerSet;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound();
        }
        catch (DbUpdateException)
        {
            if (await db.ExerciseTypes.AnyAsync(item => item.Id == form.ExerciseTypeId, cancellationToken))
            {
                throw;
            }

            ModelState.AddModelError(nameof(WorkoutExercise.ExerciseTypeId), "Wybrany typ ćwiczenia został usunięty. Wybierz inny.");
            return await ShowFormAsync(nameof(Edit), form, cancellationToken);
        }

        return RedirectToAction(nameof(Index), new { sessionId = exercise.WorkoutSessionId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var exercise = await OwnedExercises(id).AsNoTracking()
            .Include(item => item.ExerciseType)
            .SingleOrDefaultAsync(cancellationToken);

        return exercise is null ? NotFound() : View(exercise);
    }

    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = OwnedExercises(id);
        var sessionId = await query.Select(item => (int?)item.WorkoutSessionId).SingleOrDefaultAsync(cancellationToken);
        if (sessionId is null)
        {
            return NotFound();
        }

        var deletedRows = await query.ExecuteDeleteAsync(cancellationToken);
        return deletedRows == 0 ? NotFound() : RedirectToAction(nameof(Index), new { sessionId });
    }

    private IQueryable<WorkoutExercise> OwnedExercises(int id)
    {
        var userId = users.GetUserId(User);
        return db.WorkoutExercises.Where(item => item.Id == id && item.WorkoutSession.UserId == userId);
    }

    private async Task<bool> OwnsSessionAsync(int sessionId, CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        return userId is not null && await db.WorkoutSessions
            .AnyAsync(item => item.Id == sessionId && item.UserId == userId, cancellationToken);
    }

    private async Task<bool> ReadExerciseFormAsync(WorkoutExercise exercise, CancellationToken cancellationToken)
    {
        var form = await Request.ReadFormAsync(cancellationToken);
        // Pole typu number przesyła część dziesiętną z kropką niezależnie od języka przeglądarki.
        var formValues = new FormValueProvider(BindingSource.Form, form, CultureInfo.InvariantCulture);
        if (!await TryUpdateModelAsync(exercise, string.Empty, formValues,
                item => item.ExerciseTypeId, item => item.WeightKg, item => item.Sets, item => item.RepetitionsPerSet))
        {
            return false;
        }

        if (await db.ExerciseTypes.AnyAsync(item => item.Id == exercise.ExerciseTypeId, cancellationToken))
        {
            return true;
        }

        ModelState.AddModelError(nameof(WorkoutExercise.ExerciseTypeId), "Wybierz dostępny typ ćwiczenia.");
        return false;
    }

    private async Task<IActionResult> ShowFormAsync(string viewName, WorkoutExercise exercise, CancellationToken cancellationToken)
    {
        var exerciseTypes = await db.ExerciseTypes.AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);

        ViewData["ExerciseTypes"] = new SelectList(exerciseTypes, nameof(ExerciseType.Id), nameof(ExerciseType.Name));
        return View(viewName, exercise);
    }
}
