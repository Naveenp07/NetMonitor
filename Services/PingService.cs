using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NetworkMonitor.Services
{
    public class PingService
    {
        /// <summary>
        /// Synchronous ping (optional)
        /// </summary>
        public string CheckStatus(string ip)
        {
            try
            {
                using Ping ping = new Ping();
                var reply = ping.Send(ip, 1000);
                return reply.Status == IPStatus.Success ? "UP" : "DOWN";
            }
            catch
            {
                return "DOWN";
            }
        }

        /// <summary>
        /// Async ping method for MonitorService
        /// </summary>
        public async Task<string> CheckStatusAsync(string ip)
        {
            try
            {
                using Ping ping = new Ping();
                var reply = await ping.SendPingAsync(ip, 1000); // 1-second timeout
                return reply.Status == IPStatus.Success ? "UP" : "DOWN";
            }
            catch
            {
                return "DOWN";
            }
        }
    }
}