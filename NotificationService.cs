using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

public class NotificationService
{
    private static bool initialized = false;

    public NotificationService(IConfiguration configuration)
    {
        if (!initialized)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(configuration["Firebase:CredentialPath"])
            });
            initialized = true;
        }
    }

    public static async Task SendNotificationAsync(string title, string body)
    {
        var message = new Message()
        {
            Topic = "all",
            Notification = new Notification()
            {
                Title = title,
                Body = body
            }
        };

        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        Console.WriteLine("Successfully sent message: " + response);
    }
}

