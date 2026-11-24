using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BeFit.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel(
    SignInManager<IdentityUser> signIn,
    UserManager<IdentityUser> users,
    IWebHostEnvironment environment) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [TempData]
    public string? ErrorMessage { get; set; }

    public string? ReturnUrl { get; private set; }
    public IEnumerable<AuthenticationScheme> ExternalLogins { get; private set; } = [];

    public async Task OnGetAsync(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        ExternalLogins = await signIn.GetExternalAuthenticationSchemesAsync();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        if (ModelState.IsValid)
        {
            var result = await signIn.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return LocalRedirect(ReturnUrl ?? Url.Content("~/"));
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { returnUrl = ReturnUrl, Input.RememberMe });
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            if (result.IsNotAllowed && environment.IsDevelopment())
            {
                var user = await users.FindByNameAsync(Input.Email);
                if (user is not null && !await users.IsEmailConfirmedAsync(user) && !await users.IsLockedOutAsync(user))
                {
                    if (await users.CheckPasswordAsync(user, Input.Password))
                    {
                        TempData[nameof(RegisterConfirmationModel.ConfirmationUrl)] =
                            await AccountConfirmation.CreateUrlAsync(users, user, Url, ReturnUrl, Request.Scheme);
                        return RedirectToPage("./RegisterConfirmation");
                    }

                    await users.AccessFailedAsync(user);
                }
            }

            ModelState.AddModelError(string.Empty, "Nie udało się zalogować. Sprawdź dane i upewnij się, że konto zostało potwierdzone.");
        }

        ExternalLogins = await signIn.GetExternalAuthenticationSchemesAsync();
        return Page();
    }

    public class LoginInput
    {
        [Display(Name = "Adres e-mail")]
        [Required(ErrorMessage = "Podaj adres e-mail.")]
        [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Hasło")]
        [Required(ErrorMessage = "Podaj hasło.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
    }
}
