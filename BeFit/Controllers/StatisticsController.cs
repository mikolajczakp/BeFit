using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Controllers;

[Authorize]
public class StatisticsController(ApplicationDbContext db, UserManager<IdentityUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var now = DateTime.Now;
        var periodStart = now.AddDays(-28);
        var exercises = await db.WorkoutExercises.AsNoTracking()
            .Where(item => item.WorkoutSession.UserId == userId &&
                item.WorkoutSession.StartedAt >= periodStart &&
                item.WorkoutSession.StartedAt <= now)
            .Select(item => new
            {
                item.ExerciseTypeId,
                item.ExerciseType.Name,
                WeightKg = item.WeightKg!.Value,
                Sets = item.Sets!.Value,
                RepetitionsPerSet = item.RepetitionsPerSet!.Value
            })
            .ToListAsync(cancellationToken);

        var statistics = exercises
            .GroupBy(item => new { item.ExerciseTypeId, item.Name })
            .Select(group => new ExerciseStatistics(
                group.Key.Name,
                group.Count(),
                group.Sum(item => (long)item.Sets * item.RepetitionsPerSet),
                group.Average(item => item.WeightKg),
                group.Max(item => item.WeightKg)))
            .OrderBy(item => item.ExerciseName, StringComparer.Create(
                System.Globalization.CultureInfo.GetCultureInfo("pl-PL"), ignoreCase: true))
            .ToList();

        return View(statistics);
    }
}
