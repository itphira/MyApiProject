using System.ComponentModel.DataAnnotations.Schema;

public class Notification
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("text")]
    public string Text { get; set; }

    [Column("link")]
    public string Link { get; set; }
}
