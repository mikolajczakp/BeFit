using BeFit.Data;
using BeFit.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Controllers;

[Authorize]
public class WorkoutSessionsController(ApplicationDbContext db, UserManager<IdentityUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var sessions = await db.WorkoutSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.StartedAt)
            .ThenByDescending(session => session.Id)
            .ToListAsync(cancellationToken);

        return View(sessions);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new WorkoutSession());
    }

    [HttpPost, ActionName(nameof(Create))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateConfirmed(CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
        {
            return Forbid();
        }

        var session = new WorkoutSession { UserId = user.Id };
        if (!await TryUpdateModelAsync(session, string.Empty, item => item.StartedAt, item => item.EndedAt))
        {
            return View(nameof(Create), session);
        }

        db.WorkoutSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var session = await db.WorkoutSessions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        return session is null ? NotFound() : View(session);
    }

    [HttpPost, ActionName(nameof(Edit))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditConfirmed([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var matchingSession = db.WorkoutSessions.Where(item => item.Id == id && item.UserId == userId);
        if (!await matchingSession.AnyAsync(cancellationToken))
        {
            return NotFound();
        }

        var session = new WorkoutSession { Id = id, UserId = userId };
        if (!await TryUpdateModelAsync(session, string.Empty, item => item.StartedAt, item => item.EndedAt))
        {
            return View(nameof(Edit), session);
        }

        var updatedRows = await matchingSession.ExecuteUpdateAsync(
            setters => setters
                .SetProperty(item => item.StartedAt, session.StartedAt)
                .SetProperty(item => item.EndedAt, session.EndedAt),
            cancellationToken);

        return updatedRows == 0 ? NotFound() : RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var session = await db.WorkoutSessions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        return session is null ? NotFound() : View(session);
    }

    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id, CancellationToken cancellationToken)
    {
        var userId = users.GetUserId(User);
        if (userId is null)
        {
            return Forbid();
        }

        var deletedRows = await db.WorkoutSessions
            .Where(item => item.Id == id && item.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedRows == 0 ? NotFound() : RedirectToAction(nameof(Index));
    }
}
