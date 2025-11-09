using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzshBot.Infrastructure.Models;

public enum Season
{
    Winter,
    Spring,
    Summer,
    Autumn
}

[Table("sessions")]
public class Session
{
    [Key]
    [Column(name: "id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column(name: "year")]
    public int Year { get; set; }

    [Required]
    [Column(name: "season", TypeName = "season")]
    public Season Season { get; set; }
}