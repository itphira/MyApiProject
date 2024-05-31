using FirebaseAdmin.Messaging;
using System.Threading.Tasks;

public class NotificationService
{
    public async Task SendNotificationAsync(string token, string title, string body)
    {
        var message = new Message()
        {
            Token = token,
            Notification = new Notification()
            {
                Title = title,
                Body = body
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        // Log the response if necessary
    }

    public async Task SendNotificationToTopicAsync(string topic, string title, string body)
    {
        var message = new Message()
        {
            Topic = topic,
            Notification = new Notification()
            {
                Title = title,
                Body = body
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        // Log the response if necessary
    }
}
