using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BeFit.Models;
using BeFit.Security;

namespace BeFit.Controllers;

public class HomeController(UserManager<IdentityUser> users, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (environment.IsDevelopment() && User.Identity?.IsAuthenticated == true)
        {
            var user = await users.GetUserAsync(User);
            if (user is not null)
            {
                ViewData["AdministratorSetupEmail"] = await Administrator.GetFirstAccountSetupEmailAsync(users, user, cancellationToken);
            }
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
