using System.ComponentModel.DataAnnotations.Schema;

public class ReplyNotificationRequest
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("message")]
    public string Message { get; set; }

    [Column("receiver_username")]
    public string ReceiverUsername { get; set; }
}
