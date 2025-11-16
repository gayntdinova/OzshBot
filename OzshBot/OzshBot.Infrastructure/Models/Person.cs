using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Infrastructure.Enums;

namespace OzshBot.Infrastructure.Models;

[Table("people")]
public class Person
{
    [Key]
    [Column(name: "person_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid PersonId { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey("User")]
    [Column(name: "user_id")]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    [Column(name: "role", TypeName = "role")]
    public Role? Role { get; set; }

    [Required]
    [Column(name: "name")]
    public required string Name { get; set; }
    [Required]
    [Column(name: "surname")]
    public required string Surname { get; set; }
    [Column(name: "patronymic")]
    public string? Patronymic { get; set; }

    [Column(name: "city")]
    public string? City { get; set; }
    [Column(name: "school")]
    public string? School { get; set; }

    [Column(name: "birth_date")]
    public DateOnly? BirthDate { get; set; }

    [Column(name: "current_class")]
    public int CurrentClass { get; set; }
    [Column(name: "current_group")]
    public int CurrentGroup { get; set; }

    [Column(name: "email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Column(name: "phone")]
    [Phone]
    public string? Phone { get; set; }

    public virtual List<ChildParent>? Relations { get; set; }
    [NotMapped]
    public List<Parent>? Parents => Relations?.Select(r => r.Parent).ToList();
}
