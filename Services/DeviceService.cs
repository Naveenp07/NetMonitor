using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using NetworkMonitor.Models;

namespace NetworkMonitor.Services
{
    public class DeviceService
    {
        // In-memory list for testing / demo purposes
        public List<Device> Devices { get; } = new List<Device>
        {
            new Device { Id = 1, Name = "Google DNS", IPAddress = "8.8.8.8", Status = "Unknown" },
            new Device { Id = 2, Name = "Router", IPAddress = "192.168.0.1", Status = "Unknown" }
        };

        // Optional: method to get devices
        public List<Device> GetDevices() => Devices;

        // Optional: add a device
        public void AddDevice(Device device) => Devices.Add(device);

        // Optional: remove a device
        public bool RemoveDevice(int id)
        {
            var device = Devices.Find(d => d.Id == id);
            if (device != null)
            {
                Devices.Remove(device);
                return true;
            }
            return false;
        }
    }
}