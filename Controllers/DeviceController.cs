using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Models;   // Device, DeviceLog
using NetworkMonitor.Services; // PingService
using NetworkMonitor.Data;     // AppDbContext

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

        // GET: api/device
        [HttpGet]
        public async Task<ActionResult<List<Device>>> GetDevices()
        {
            var devices = await _context.Devices.ToListAsync();

            foreach (var device in devices)
            {
                var status = _pingService.CheckStatus(device.IPAddress);

                if (device.Status != status)
                {
                    _context.DeviceLogs.Add(new DeviceLog
                    {
                        DeviceId = device.Id,
                        Status = status,
                        Timestamp = DateTime.Now
                    });
                }

                device.Status = status;
            }

            await _context.SaveChangesAsync();

            return devices;
        }
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


        // POST: api/device
        [HttpPost]
        public async Task<ActionResult<Device>> AddDevice(Device device)
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDevices), new { id = device.Id }, device);
        }

        // DELETE: api/device/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}