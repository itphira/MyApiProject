using System.ComponentModel.DataAnnotations.Schema;

public class Company
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public String Title { get; set; }

    [Column("image", TypeName = "bytea")]
    public byte[] Image { get; set; }
}

