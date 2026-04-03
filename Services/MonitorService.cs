using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Data;
using NetworkMonitor.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkMonitor.Services
{
    public class MonitorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PingService _pingService;

        public MonitorService(IServiceProvider serviceProvider, PingService pingService)
        {
            _serviceProvider = serviceProvider;
            _pingService = pingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Step 1: Ping entire subnet to find active devices
                    var activeIPs = await GetActiveIPsAsync();

                    var tasks = new List<Task>();

                    foreach (var ip in activeIPs)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            // Get existing device or add new
                            var device = await _context.Devices
                                .FirstOrDefaultAsync(d => d.IPAddress == ip, stoppingToken);

                            if (device == null)
                            {
                                device = new Device
                                {
                                    Name = await ResolveHostNameAsync(ip),
                                    IPAddress = ip,
                                    Status = "Unknown"
                                };

                                _context.Devices.Add(device);
                                await _context.SaveChangesAsync(stoppingToken); // ensure Id is generated
                            }

                            // Check status asynchronously
                            var status = await _pingService.CheckStatusAsync(ip);

                            // Log status
                            _context.DeviceLogs.Add(new DeviceLog
                            {
                                DeviceId = device.Id,
                                Status = status,
                                Timestamp = DateTime.Now
                            });

                            // Send alert if device goes down
                            if (device.Status != status && status == "DOWN")
                            {
                                AlertService.SendEmailAlert(device);
                                AlertService.SendTelegramAlert(device);
                            }

                            device.Status = status;
                        }));
                    }

                    await Task.WhenAll(tasks);

                    // Save all logs in one batch
                    await _context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"MonitorService error: {ex.Message}");
                }

                // Wait 15 seconds before next cycle
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }

        // Async ping sweep to detect active IPs in subnet
        private async Task<List<string>> GetActiveIPsAsync()
        {
            var activeIPs = new List<string>();
            string baseIp = GetBaseIp();

            var tasks = new List<Task>();

            for (int i = 1; i <= 254; i++)
            {
                string ip = baseIp + i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var status = await _pingService.CheckStatusAsync(ip);
                        if (status == "UP")
                        {
                            lock (activeIPs)
                            {
                                activeIPs.Add(ip);
                            }
                        }
                    }
                    catch { }
                }));
            }

            await Task.WhenAll(tasks);
            return activeIPs;
        }

        // Determine base subnet automatically (e.g., 192.168.1.)
        private string GetBaseIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    var parts = ip.ToString().Split('.');
                    return $"{parts[0]}.{parts[1]}.{parts[2]}.";
                }
            }

            return "192.168.0."; // fallback
        }

        // Async hostname resolution
        private async Task<string> ResolveHostNameAsync(string ip)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(ip);
                return entry.HostName;
            }
            catch
            {
                return ip;
            }
        }
    }
}