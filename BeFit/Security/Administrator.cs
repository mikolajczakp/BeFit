using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BeFit.Security;

public static class Administrator
{
    public const string Role = "Administrator";
    public const string GrantCommand = "--grant-admin";

    public static async Task<string?> GetFirstAccountSetupEmailAsync(
        UserManager<IdentityUser> users, IdentityUser user, CancellationToken cancellationToken)
    {
        if (await users.IsInRoleAsync(user, Role) ||
            await users.Users.AnyAsync(account => account.Id != user.Id, cancellationToken))
        {
            return null;
        }

        return user.Email;
    }

    public static async Task<int> GrantAsync(IServiceProvider services, string email)
    {
        using var scope = services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var user = await users.FindByEmailAsync(email);
        if (user is null || !await users.IsEmailConfirmedAsync(user))
        {
            Console.Error.WriteLine("Najpierw zarejestruj konto z tym adresem i potwierdź je.");
            return 1;
        }

        if (!await roles.RoleExistsAsync(Role))
        {
            var created = await roles.CreateAsync(new IdentityRole(Role));
            if (!created.Succeeded)
            {
                return ReportErrors(created);
            }
        }

        if (!await users.IsInRoleAsync(user, Role))
        {
            var assigned = await users.AddToRoleAsync(user, Role);
            if (!assigned.Succeeded)
            {
                return ReportErrors(assigned);
            }
        }

        Console.WriteLine("Konto ma rolę administratora. Zaloguj się ponownie.");
        return 0;
    }

    private static int ReportErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine(error.Description);
        }

        return 1;
    }
}
