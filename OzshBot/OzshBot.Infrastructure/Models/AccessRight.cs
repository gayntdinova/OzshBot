using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzshBot.Infrastructure.Models;

public enum Access
{
    Read,
    WriteRead
}

[Table("access_rights")]
public class AccessRight
{
    [Key]
    [ForeignKey("User")]
    [Column(name: "user_id")]
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    [Required]
    [Column(name: "rights", TypeName = "access")]
    public Access Rights { get; set; } = Access.Read;
}