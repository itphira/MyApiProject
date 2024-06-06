using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MyApiProject
{
    public class NotificationService
    {
        private readonly IConfiguration _configuration;

        public NotificationService(IConfiguration configuration)
        {
            _configuration = configuration;
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

            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to send notification: {response.StatusCode}, Response: {responseString}");
            }
        }
    }
}
