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

    [Required]
    [Column(name: "city")]
    public required string City { get; set; }
    [Required]
    [Column(name: "school")]
    public required string School { get; set; }

    [Required]
    [Column(name: "birth_date")]
    public DateOnly BirthDate { get; set; }

    [Column(name: "current_class")]
    public int CurrentClass { get; set; }
    [Column(name: "current_group")]
    public int? CurrentGroup { get; set; }

    [Required]
    [Column(name: "email")]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [Column(name: "phone")]
    [Phone]
    public required string Phone { get; set; }

    public virtual List<ChildParent>? Relations { get; set; }
    [NotMapped]
    public List<Parent> Parents => Relations?.Select(r => r.Parent).ToList() ?? [];
}


public static class StudentConverter
{
    public static Domain.Entities.ChildInfo ToChildInfo(this Student student)
    {
        var fullName = new FullName
        {
            Name = student.Name,
            Surname = student.Surname,
            Patronymic = student.Patronymic
        };
        var educationInfo = new EducationInfo { Class = student.CurrentClass, School = student.School };
        var result = new Domain.Entities.ChildInfo
        {
            Id = student.StudentId,
            FullName = fullName,
            Birthday = student.BirthDate,
            City = student.City,
            PhoneNumber = student.Phone,
            Email = student.Email,
            Group = student.CurrentGroup,
            EducationInfo = educationInfo,
            Parents = student.Parents.Select(p => p.ToParentInfo()).ToArray(),
            Sessions = []
        };
        return result;
    }

    public static Domain.Entities.User ToDomainUser(this Student student)
    {
        var tgInfo = new TelegramInfo { TgUsername = student.User.TgName, TgId = student.User.TgId };
        return new Domain.Entities.User
        {
            Id = student.User.UserId,
            TelegramInfo = tgInfo,
            ChildInfo = student.ToChildInfo(),
            Role = Domain.Enums.Role.Child
        };
    }

    public static Student? FromChildInfo(ChildInfo? childInfo)
    {
        if (childInfo == null) return null;
        var parents = childInfo.Parents?.Select(p => ParentConverter.FromParentInfo(p)).ToList();
        return new Student
        {
            StudentId = childInfo.Id,
            Name = childInfo.FullName.Name,
            Surname = childInfo.FullName.Surname,
            Patronymic = childInfo.FullName.Patronymic,
            City = childInfo.City,
            School = childInfo.EducationInfo.School,
            CurrentClass = childInfo.EducationInfo.Class,
            BirthDate = childInfo.Birthday,
            CurrentGroup = childInfo.Group,
            Email = childInfo.Email,
            Phone = childInfo.PhoneNumber,
        };
    }
    
    public static Student? FromChildInfo(ChildInfo? childInfo, Domain.Entities.User user)
    {
        if (childInfo == null) return null;
        var parents = childInfo.Parents?.Select(p => ParentConverter.FromParentInfo(p)).ToList();
        return new Student
        {
            UserId = user.Id,
            StudentId = childInfo.Id,
            Name = childInfo.FullName.Name,
            Surname = childInfo.FullName.Surname,
            Patronymic = childInfo.FullName.Patronymic,
            City = childInfo.City,
            School = childInfo.EducationInfo.School,
            CurrentClass = childInfo.EducationInfo.Class,
            BirthDate = childInfo.Birthday,
            CurrentGroup = childInfo.Group,
            Email = childInfo.Email,
            Phone = childInfo.PhoneNumber,
        };
    }
}
