using System.Text;
using BeFit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BeFit.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ConfirmEmailModel(UserManager<IdentityUser> users, IWebHostEnvironment environment) : PageModel
{
    public bool Confirmed { get; private set; }
    public string? ReturnUrl { get; private set; }
    public string? AdministratorSetupEmail { get; private set; }

    public async Task OnGetAsync(string? userId, string? code, string? returnUrl)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
        {
            return;
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return;
        }

        var user = await users.FindByIdAsync(userId);
        if (user is not null)
        {
            Confirmed = (await users.ConfirmEmailAsync(user, token)).Succeeded;
            if (Confirmed && environment.IsDevelopment())
            {
                AdministratorSetupEmail = await Administrator.GetFirstAccountSetupEmailAsync(users, user, HttpContext.RequestAborted);
            }
        }
    }
}
