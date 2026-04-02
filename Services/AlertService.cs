using NetworkMonitor.Models;
using System.Net.Http;
using System.Net.Mail;

namespace NetworkMonitor.Services
{
    public static class AlertService
    {
        public static void SendEmailAlert(Device device)
        {
            try
            {
                var client = new SmtpClient("smtp.yourserver.com")
                {
                    Port = 587,
                    Credentials = new System.Net.NetworkCredential("your@email.com", "password"),
                    EnableSsl = true
                };

                client.Send("your@email.com", "admin@email.com",
                    $"Device DOWN: {device.Name}", $"The device {device.Name} ({device.IPAddress}) is DOWN!");
            }
            catch { }
        }

        public static void SendTelegramAlert(Device device)
        {
            var botToken = "YOUR_BOT_TOKEN";
            var chatId = "YOUR_CHAT_ID";
            var message = $"Device DOWN: {device.Name} ({device.IPAddress})";

            using var client = new HttpClient();
            client.GetAsync($"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={message}").Wait();
        }
    }
}