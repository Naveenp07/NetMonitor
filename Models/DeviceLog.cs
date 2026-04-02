namespace NetworkMonitor.Models
{
    public class DeviceLog
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}