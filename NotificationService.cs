using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;
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

        public async Task SendNotificationAsync(string title, string message)
        {
            var projectId = _configuration["Firebase:ProjectId"];
            var serviceAccountKeyPath = _configuration["Firebase:ServiceAccountKeyPath"];
            var url = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";

            var data = new
            {
                message = new
                {
                    topic = "articles",
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
            using (var stream = new FileStream(serviceAccountKeyPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            }

            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");

            try
            {
                _logger.LogInformation("Sending notification to FCM...");
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
        public async Task SendReplyNotificationAsync(string toUsername, string fromUsername, string message)
        {
            var projectId = _configuration["Firebase:ProjectId"];
            var serviceAccountKeyPath = _configuration["Firebase:ServiceAccountKeyPath"];
            var url = $"https://fcm.googleapis.com/v1/projects/{projectId}/messages:send";

            var data = new
            {
                message = new
                {
                    topic = toUsername,
                    notification = new
                    {
                        title = "New Reply to Your Comment",
                        body = $"{fromUsername} replied: {message}"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            GoogleCredential credential;
            using (var stream = new FileStream(serviceAccountKeyPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            }

            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");

            try
            {
                _logger.LogInformation("Sending reply notification to FCM...");
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to send reply notification: {response.StatusCode}, Response: {responseString}");
                    throw new HttpRequestException($"Failed to send reply notification: {response.StatusCode}, Response: {responseString}");
                }

                _logger.LogInformation($"Reply notification sent successfully: {responseString}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception in SendReplyNotificationAsync: {ex.Message}");
                throw;
            }
        }

    }

}
