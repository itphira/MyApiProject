public class Comment
{
    public int CommentId { get; set; }
    public int ArticleId { get; set; }
    public int? ParentId { get; set; }  // Nullable int for parent comment ID
    public string Author { get; set; }
    public string CommentText { get; set; }
    public DateTime PostedDate { get; set; }
}
