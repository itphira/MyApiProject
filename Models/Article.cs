using System.ComponentModel.DataAnnotations;

public class Article
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    [Required]
    public string Text { get; set; }

    public byte[] Image { get; set; } // Consider limiting the size
}
