using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Data;
using NetworkMonitor.Models;

namespace NetworkMonitor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceLogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeviceLogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/devicelog
        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            // join DeviceLogs with Devices manually
            var logs = await _context.DeviceLogs
                .Join(_context.Devices,
                      log => log.DeviceId,
                      device => device.Id,
                      (log, device) => new
                      {
                          id = log.Id,
                          deviceName = device.Name,
                          ipAddress = device.IPAddress,
                          status = log.Status,
                          timestamp = log.Timestamp
                      })
                .OrderBy(l => l.timestamp)
                .ToListAsync();

            return Ok(logs);
        }
    }
}