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
        public async Task<ActionResult<List<DeviceLog>>> GetLogs()
        {
            var logs = await _context.DeviceLogs
                .OrderBy(d => d.Timestamp)
                .ToListAsync();

            return Ok(logs);
        }
    }
}