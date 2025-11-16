using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzshBot.Infrastructure.Models;

[Table("users")]
public class User
{
    [Key]
    [Column(name: "user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid UserId { get; set; }

    [Required]
    [Column(name: "tg_name")]
    public required string TgName { get; set; }
    [Column(name: "tg_id")]
    public long TgId { get; set; }

    public virtual Person? Person { get; set; }
    public virtual AccessRight? AccessRight { get; set; }
}
