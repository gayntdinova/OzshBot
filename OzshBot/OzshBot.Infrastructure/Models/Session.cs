using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Domain.Enums;

namespace OzshBot.Infrastructure.Models;

[Table("sessions")]
public class Session
{
    [Key]
    [Column(name: "id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column(name: "start_date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [Column(name: "end_date")]
    public DateOnly EndDate { get; set; }
}
