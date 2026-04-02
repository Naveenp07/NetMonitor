namespace NetworkMonitor.Models
{
    public class Device
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Unknown";
    }
}