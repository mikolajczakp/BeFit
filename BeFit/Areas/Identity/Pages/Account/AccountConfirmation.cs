using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace BeFit.Areas.Identity.Pages.Account;

internal static class AccountConfirmation
{
    internal static async Task<string> CreateUrlAsync(
        UserManager<IdentityUser> users, IdentityUser user, IUrlHelper url, string? returnUrl, string scheme)
    {
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        return url.Page("./ConfirmEmail", null, new { userId = user.Id, code, returnUrl }, scheme)
            ?? throw new InvalidOperationException("Nie można utworzyć linku potwierdzającego konto.");
    }
}
