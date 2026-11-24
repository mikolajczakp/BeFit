using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BeFit.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(
    UserManager<IdentityUser> users,
    SignInManager<IdentityUser> signIn,
    IEmailSender emailSender,
    IWebHostEnvironment environment) : PageModel
{
    [BindProperty]
    public RegistrationInput Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }
    public IEnumerable<AuthenticationScheme> ExternalLogins { get; private set; } = [];

    public async Task OnGetAsync(string? returnUrl)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        ExternalLogins = await signIn.GetExternalAuthenticationSchemesAsync();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        await OnGetAsync(returnUrl);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        var result = await users.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                var message = error.Code switch
                {
                    "DuplicateUserName" or "DuplicateEmail" => "Konto z tym adresem e-mail już istnieje.",
                    "InvalidUserName" or "InvalidEmail" => "Podaj poprawny adres e-mail.",
                    "PasswordRequiresDigit" => "Hasło musi zawierać cyfrę.",
                    "PasswordRequiresLower" => "Hasło musi zawierać małą literę.",
                    "PasswordRequiresUpper" => "Hasło musi zawierać wielką literę.",
                    "PasswordRequiresNonAlphanumeric" => "Hasło musi zawierać znak specjalny.",
                    _ => "Nie udało się utworzyć konta. Sprawdź podane dane."
                };
                ModelState.AddModelError(string.Empty, message);
            }

            return Page();
        }

        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmUrl = Url.Page("./ConfirmEmail", null, new { userId = user.Id, code, returnUrl = ReturnUrl }, Request.Scheme)
            ?? throw new InvalidOperationException("Nie można utworzyć linku potwierdzającego konto.");

        await emailSender.SendEmailAsync(Input.Email, "Potwierdź konto BeFit",
            $"<a href=\"{HtmlEncoder.Default.Encode(confirmUrl)}\">Potwierdź konto BeFit</a>");

        if (users.Options.SignIn.RequireConfirmedAccount)
        {
            if (environment.IsDevelopment())
            {
                TempData[nameof(RegisterConfirmationModel.ConfirmationUrl)] = confirmUrl;
            }

            return RedirectToPage("./RegisterConfirmation");
        }

        await signIn.SignInAsync(user, isPersistent: false);
        return LocalRedirect(ReturnUrl ?? Url.Content("~/"));
    }

    public class RegistrationInput
    {
        [Display(Name = "Adres e-mail")]
        [Required(ErrorMessage = "Podaj adres e-mail.")]
        [EmailAddress(ErrorMessage = "Podaj poprawny adres e-mail.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Hasło")]
        [Required(ErrorMessage = "Podaj hasło.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Hasło musi mieć od {2} do {1} znaków.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Powtórz hasło")]
        [Required(ErrorMessage = "Wpisz hasło ponownie.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Hasła muszą być takie same.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
