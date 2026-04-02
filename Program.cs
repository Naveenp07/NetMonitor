using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Models;
using NetworkMonitor.Services;
using NetworkMonitor.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add your app services
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<PingService>();

// Background monitoring service
builder.Services.AddHostedService<MonitorService>();

// Database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=NetworkMonitorDB;Trusted_Connection=True;TrustServerCertificate=True"));

var app = builder.Build();

// Serve static files
app.UseDefaultFiles();
app.UseStaticFiles();

// Map API controllers
app.MapControllers();

app.Run();