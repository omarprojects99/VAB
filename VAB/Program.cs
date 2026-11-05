using Microsoft.EntityFrameworkCore;
using VAB.Data;
using Microsoft.AspNetCore.Identity;
using VAB.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using VAB.Hubs;
using VAB.Services;
using VAB.Extensions;
//using TextExtraction; pdf extractor
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTwilioServices();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ITrouDeCaisseService, TrouDeCaisseService>();
builder.Services.AddScoped<IRedevanceService, RedevanceService>();
builder.Services.AddScoped<IRedevanceAnalyticsService, RedevanceAnalyticsService>();
builder.Services.AddSignalR(); // Add SignalR
builder.Services.AddHttpClient();
builder.Services.AddLogging(logging =>
{
    logging.AddConsole(); // Log to console for debugging
    logging.AddDebug();
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

//#region Test===================================================================================================================
//var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
//var logger = loggerFactory.CreateLogger<PdfTextExtractor>();
//var extractor = new PdfTextExtractor(logger);
//var textSections = await extractor.ExtractAsync("file.pdf");
//foreach (var section in textSections)
//{
//    Console.WriteLine($"Title: {section.Title}, Type: {section.Type}, Content: {section.Content}");
//}
//#endregion                                                   pdf extractor

var app = builder.Build();

var adminEmail = app.Configuration["AdminBootstrap:Email"]?.Trim();
var adminPassword = app.Configuration["AdminBootstrap:Password"];

if (!app.Environment.IsDevelopment())
{
    if (!string.IsNullOrWhiteSpace(adminEmail) || !string.IsNullOrWhiteSpace(adminPassword))
    {
        app.Logger.LogWarning("Admin bootstrap settings are ignored outside Development.");
    }
}
else if (string.IsNullOrWhiteSpace(adminEmail) && string.IsNullOrWhiteSpace(adminPassword))
{
    app.Logger.LogInformation("Admin bootstrap skipped. Configure local User Secrets or environment variables to provision a development administrator.");
}
else if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
{
    throw new InvalidOperationException("Both AdminBootstrap:Email and AdminBootstrap:Password must be configured.");
}
else
{
    await using var scope = app.Services.CreateAsyncScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        var roleResult = await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException($"Could not create the Admin role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
        }
    }

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            IsActive = true,
            NomPrenom = "Super Admin"
        };

        var userResult = await userManager.CreateAsync(admin, adminPassword);
        if (!userResult.Succeeded)
        {
            throw new InvalidOperationException($"Could not create the administrator account: {string.Join("; ", userResult.Errors.Select(error => error.Description))}");
        }
    }
    else if (app.Configuration.GetValue<bool>("AdminBootstrap:ResetPasswordOnStartup"))
    {
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
        var resetResult = await userManager.ResetPasswordAsync(admin, resetToken, adminPassword);
        if (!resetResult.Succeeded)
        {
            throw new InvalidOperationException($"Could not reset the bootstrap administrator password: {string.Join("; ", resetResult.Errors.Select(error => error.Description))}");
        }

        admin.IsActive = true;
        admin.EmailConfirmed = true;
        await userManager.SetLockoutEndDateAsync(admin, null);
        await userManager.ResetAccessFailedCountAsync(admin);
        var updateResult = await userManager.UpdateAsync(admin);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException($"Could not update the bootstrap administrator account: {string.Join("; ", updateResult.Errors.Select(error => error.Description))}");
        }
    }

    if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        var roleAssignment = await userManager.AddToRoleAsync(admin, "Admin");
        if (!roleAssignment.Succeeded)
        {
            throw new InvalidOperationException($"Could not assign the Admin role: {string.Join("; ", roleAssignment.Errors.Select(error => error.Description))}");
        }
    }

    app.Logger.LogInformation("Administrator account bootstrap completed for {Email}.", adminEmail);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapHub<NotificationHub>("/notificationHub");
// Configure culture
var supportedCultures = new[] { new CultureInfo("fr-FR") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("fr-FR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
//app.Run("http://0.0.0.0:5000");
app.Run();
