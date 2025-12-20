using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

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
    public DateOnly BirthDate { get; set; }

    [Column(name: "current_class")]
    public int CurrentClass { get; set; }
    [Column(name: "current_group")]
    public int? CurrentGroup { get; set; }

    [Column(name: "email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [Column(name: "phone")]
    [Phone]
    public required string Phone { get; set; }

    public virtual List<StudentSession>? SessionRelations { get; set; }
    [NotMapped]
    public List<Session> Sessions => SessionRelations?.Select(r => r.Session).ToList() ?? [];

    public virtual List<ChildParent>? ParentRelations { get; set; }
    [NotMapped]
    public List<Parent> Parents => ParentRelations?.Select(r => r.Parent).ToList() ?? [];
}


public static class StudentConverter
{
    public static ChildInfo ToChildInfo(this Student student)
    {
        var educationInfo = new EducationInfo { Class = student.CurrentClass, School = student.School };
        var result = new ChildInfo
        {
            Group = student.CurrentGroup,
            EducationInfo = educationInfo,
            ContactPeople = student.Parents.Select(p => p.ToContactPerson()).ToHashSet(),
            Sessions = student.Sessions.Select(s => s.ToDomainSession()).ToHashSet()
        };
        return result;
    }

    public static void UpdateFromChildInfo(this Student student, ChildInfo childInfo)
    {
        student.School = childInfo.EducationInfo.School;
        student.CurrentClass = childInfo.EducationInfo.Class;
        student.CurrentGroup = childInfo.Group;
    }
}
