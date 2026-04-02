using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Models;

namespace NetworkMonitor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Device> Devices { get; set; } = null!;
        public DbSet<DeviceLog> DeviceLogs { get; set; } = null!;
    }
}