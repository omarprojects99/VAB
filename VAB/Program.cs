using Microsoft.EntityFrameworkCore;
using VAB.Data;
using Microsoft.AspNetCore.Identity;
using VAB.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using VAB.Hubs;
using VAB.Services;
//using TextExtraction; pdf extractor
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapHub<NotificationHub>("/notificationHub");
// add first Admin
//using (var scope = app.Services.CreateScope())
//{
//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

//    string adminEmail = "admin@admin.com";
//    string password = "[REDACTED]";

//    if (await userManager.FindByEmailAsync(adminEmail) == null)
//    {
//        var user = new User { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, NomPrenom = "Super Admin", Role = "Admin" };
//        await userManager.CreateAsync(user, password);
//    }
//}
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
