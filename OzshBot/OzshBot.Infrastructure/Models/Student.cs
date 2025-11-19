using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Infrastructure.Enums;

namespace OzshBot.Infrastructure.Models;

[Table("students")]
public class Student
{
    [Key]
    [Column(name: "student_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid StudentId { get; set; } = Guid.NewGuid();

    [Required]
    [ForeignKey("User")]
    [Column(name: "user_id")]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

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

    [Required]
    [Column(name: "birth_date")]
    public DateOnly? BirthDate { get; set; }

    [Column(name: "current_class")]
    public int CurrentClass { get; set; }
    [Column(name: "current_group")]
    public int CurrentGroup { get; set; }

    [Required]
    [Column(name: "email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [Column(name: "phone")]
    [Phone]
    public string? Phone { get; set; }

    public virtual List<ChildParent>? Relations { get; set; }
    [NotMapped]
    public List<Parent>? Parents => Relations?.Select(r => r.Parent).ToList();
}
