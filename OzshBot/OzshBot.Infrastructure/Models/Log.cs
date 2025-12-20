using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace OzshBot.Infrastructure.Models;

[Table("logs")]
public class Log
{
    [Key]
    [Column(name: "log_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LogId { get; set; }

    [Column(name: "date")]
    public DateOnly Date { get; set; }

    [Column(name: "success")]
    public required bool Success { get; set; }

    [Column(name: "tg_id")]
    public long TgId { get; set; }
}