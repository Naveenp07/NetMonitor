using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Models;
using NetworkMonitor.Services;
using NetworkMonitor.Data;

namespace NetworkMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PingService _pingService;

        public DeviceController(AppDbContext context, PingService pingService)
        {
            _context = context;
            _pingService = pingService;
        }

        // 🔹 GET: api/device
        [HttpGet]
        public async Task<IActionResult> GetDevices()
        {
            var devices = await _context.Devices.ToListAsync();

            // Update status for each device based on PingService
            foreach (var device in devices)
            {
                var status = _pingService.CheckStatus(device.IPAddress);

                // Log status change
                if (device.Status != status)
                {
                    _context.DeviceLogs.Add(new DeviceLog
                    {
                        DeviceId = device.Id,
                        Status = status,
                        Timestamp = DateTime.Now
                    });
                    device.Status = status;
                }
            }

            await _context.SaveChangesAsync();

            // Return JSON with camelCase properties for JS frontend
            var result = devices.Select(d => new
            {
                id = d.Id,
                name = d.Name,
                ipAddress = d.IPAddress,
                status = d.Status
            });

            return Ok(result);
        }

        // 🔹 GET: api/device/stats (for dashboard charts)
        [HttpGet("stats")]
        public async Task<IActionResult> GetDeviceStats()
        {
            var devices = await _context.Devices.ToListAsync();
            var upCount = devices.Count(d => d.Status == "UP");
            var downCount = devices.Count(d => d.Status == "DOWN");

            return Ok(new
            {
                labels = new[] { "UP", "DOWN" },
                data = new[] { upCount, downCount }
            });
        }

        // 🔹 POST: api/device (add new device)
        [HttpPost]
        public async Task<IActionResult> AddDevice([FromBody] Device device)
        {
            if (string.IsNullOrWhiteSpace(device.Name) || string.IsNullOrWhiteSpace(device.IPAddress))
            {
                return BadRequest("Name and IP Address are required.");
            }

            device.Status = "UNKNOWN";
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            var result = new
            {
                id = device.Id,
                name = device.Name,
                ipAddress = device.IPAddress,
                status = device.Status
            };

            return CreatedAtAction(nameof(GetDevices), new { id = device.Id }, result);
        }

        // 🔹 DELETE: api/device/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}