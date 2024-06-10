using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MyApiProject.Services
{
    public class NotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendNotificationAsync(string title, string message)
        {
            var firebaseServerKey = _configuration["Firebase:ServerKey"];
            var firebaseSenderId = _configuration["Firebase:SenderId"];
            var url = "https://fcm.googleapis.com/fcm/send";

            var data = new
            {
                to = "/topics/all",
                notification = new
                {
                    title,
                    body = message
                }
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={firebaseServerKey}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Sender", $"id={firebaseSenderId}");

            try
            {
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to send notification: {response.StatusCode}, Response: {responseString}");
                    throw new HttpRequestException($"Failed to send notification: {response.StatusCode}, Response: {responseString}");
                }

                _logger.LogInformation($"Notification sent successfully: {responseString}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in SendNotificationAsync: {ex.Message}");
                throw;
            }
        }
    }
}
