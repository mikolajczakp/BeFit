using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BeFit.Data;
using BeFit.Security;

var grantAdmin = args.FirstOrDefault() == Administrator.GrantCommand;
if (grantAdmin && (args.Length != 2 || string.IsNullOrWhiteSpace(args[1])))
{
    Console.Error.WriteLine("Użycie: dotnet run --project BeFit -- --grant-admin ADRES_EMAIL");
    Environment.ExitCode = 1;
    return;
}

var builder = WebApplication.CreateBuilder(grantAdmin ? [] : args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (grantAdmin)
{
    await using (app)
    {
        Environment.ExitCode = await Administrator.GrantAsync(app.Services, args[1]);
    }

    return;
}

if (app.Environment.IsDevelopment())
{
    // Świeża kopia projektu tworzy lokalną bazę przy pierwszym uruchomieniu.
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await database.Database.MigrateAsync();

    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
