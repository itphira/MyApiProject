using System.ComponentModel.DataAnnotations.Schema;

public class Notification
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public String Title { get; set; }
    
    [Column("text")]
    public String Text { get; set; }

}

