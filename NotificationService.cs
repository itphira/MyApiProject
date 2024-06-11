using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;

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

        public async Task SendNotificationAsync(string title, string message, StringBuilder logMessages)
        {
            var projectId = _configuration["Firebase:ProjectId"];
            var serviceAccountKeyPath = _configuration["Firebase:ServiceAccountKeyPath"];
            var url = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";

            var data = new
            {
                message = new
                {
                    topic = "all",
                    notification = new
                    {
                        title,
                        body = message
                    }
                }
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            GoogleCredential credential;
            try
            {
                using (var stream = new FileStream(serviceAccountKeyPath, FileMode.Open, FileAccess.Read))
                {
                    logMessages.AppendLine("Loading Google credentials...");
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
                }

                var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
                logMessages.AppendLine("Retrieved access token.");

                using var client = new HttpClient();
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");

                logMessages.AppendLine("Sending notification to FCM...");
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    logMessages.AppendLine($"Failed to send notification: {response.StatusCode}, Response: {responseString}");
                    throw new HttpRequestException($"Failed to send notification: {response.StatusCode}, Response: {responseString}");
                }

                logMessages.AppendLine($"Notification sent successfully: {responseString}");
            }
            catch (Exception ex)
            {
                logMessages.AppendLine($"Exception in SendNotificationAsync: {ex.Message}");
                _logger.LogError($"Exception in SendNotificationAsync: {ex.Message}");
                throw;
            }
        }
    }
}
