using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NetworkMonitor.Data;
using NetworkMonitor.Models;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace NetworkMonitor.Services
{
    public class MonitorService : BackgroundService
    {
        private readonly AppDbContext _context;
        private readonly PingService _pingService;

        public MonitorService(AppDbContext context, PingService pingService)
        {
            _context = context;
            _pingService = pingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // 🔥 STEP 1: Warm up ARP (discover devices)
                await PingSubnet();

                // 🔥 STEP 2: Get active IPs from ARP
                var activeIPs = GetActiveIPs();

                foreach (var ip in activeIPs)
                {
                    var device = await _context.Devices
                        .FirstOrDefaultAsync(d => d.IPAddress == ip);

                    // 🔹 Add new device if not exists
                    if (device == null)
                    {
                        device = new Device
                        {
                            Name = ResolveHostName(ip),
                            IPAddress = ip,
                            Status = "Unknown"
                        };

                        _context.Devices.Add(device);
                    }

                    // 🔹 Check status
                    var status = _pingService.CheckStatus(ip);

                    // 🔹 Log every check (important for graphs)
                    _context.DeviceLogs.Add(new DeviceLog
                    {
                        DeviceId = device.Id,
                        Status = status,
                        Timestamp = DateTime.Now
                    });

                    // 🔹 Send alert if device goes DOWN
                    if (device.Status != status && status == "DOWN")
                    {
                        AlertService.SendEmailAlert(device);
                        AlertService.SendTelegramAlert(device);
                    }

                    device.Status = status;
                }

                // ✅ Save once (better performance)
                await _context.SaveChangesAsync();

                // 🔁 Run every 15 seconds
                await Task.Delay(15000, stoppingToken);
            }
        }

        // 🔥 Scan full subnet to populate ARP table
        private async Task PingSubnet()
        {
            string baseIp = GetBaseIp();
            var tasks = new List<Task>();

            for (int i = 1; i <= 254; i++)
            {
                string ip = baseIp + i;

                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        using Ping ping = new Ping();
                        ping.Send(ip, 100);
                    }
                    catch { }
                }));
            }

            await Task.WhenAll(tasks);
        }

        // 🔥 Get base network IP automatically
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

        // 🔥 Read ARP table to get active devices
        private List<string> GetActiveIPs()
        {
            var result = new List<string>();

            try
            {
                Process arpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "arp",
                        Arguments = "-a",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                arpProcess.Start();
                string output = arpProcess.StandardOutput.ReadToEnd();
                arpProcess.WaitForExit();

                var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 2 && IPAddress.TryParse(parts[0], out _))
                    {
                        result.Add(parts[0]);
                    }
                }
            }
            catch
            {
                // ignore errors
            }

            return result;
        }

        // 🔥 Resolve hostname (makes UI professional)
        private string ResolveHostName(string ip)
        {
            try
            {
                var entry = Dns.GetHostEntry(ip);
                return entry.HostName;
            }
            catch
            {
                return ip;
            }
        }
    }
}