using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Models;

namespace NetworkMonitor.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Device> Devices { get; set; } = null!;
        public DbSet<DeviceLog> DeviceLogs { get; set; } = null!;
    }
}