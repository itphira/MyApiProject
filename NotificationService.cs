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
            var firebaseServerKey = _configuration["BOqNY7jh1qW9omogHIMVvAZRkS0KEMO_zZHtNAnF-jyBS7WtbzTaPIl8t40HJWN_OsbGSXlasxUZ62rQV3Hd9eI"];
            var firebaseSenderId = _configuration["1038552327464"];
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
            response.EnsureSuccessStatusCode();
        }
    }
}
