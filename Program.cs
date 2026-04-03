using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NetworkMonitor.Models;
using NetworkMonitor.Services;
using NetworkMonitor.Data;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<DeviceService>();

builder.Services.AddSingleton<PingService>();

builder.Services.AddHostedService<MonitorService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=NetworkMonitorDB;Trusted_Connection=True;TrustServerCertificate=True"));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.Run();