using System.ComponentModel.DataAnnotations.Schema;

public class Notification
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("text")]
    public string Text { get; set; }

    [Column("article_id")]
    public int ArticleId { get; set; }
    
    [Column("company_id")]
    public int CompanyId { get; set; }
}
