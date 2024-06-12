using System;
using System.ComponentModel.DataAnnotations.Schema;

public class Article
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("text")]
    public string Text { get; set; }

    [Column("image", TypeName = "bytea")]
    public byte[] Image { get; set; }

    [Column("parent_id")]
    public int ParentId { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; }
}
