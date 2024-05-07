public class Comment
{
    public int CommentId { get; set; }
    public int ArticleId { get; set; }
    public string Author { get; set; }
    public string CommentText { get; set; }
    public DateTime PostedDate { get; set; }

    public Article Article { get; set; }
}
