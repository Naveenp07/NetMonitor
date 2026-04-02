using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace NetworkMonitor.Services
{
    public class PingService
    {
        /// <summary>
        /// Checks if a device with the given IP is reachable.
        /// </summary>
        /// <param name="ip">IP address of the device</param>
        /// <returns>"UP" if reachable, "DOWN" otherwise</returns>
        public string CheckStatus(string ip)
        {
            try
            {
                using Ping ping = new Ping();
                var reply = ping.Send(ip, 1000); // 1-second timeout
                return reply.Status == IPStatus.Success ? "UP" : "DOWN";
            }
            catch
            {
                return "DOWN";
            }
        }
    }
}