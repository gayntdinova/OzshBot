using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

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
    public long? TgId { get; set; }

    [Column(name: "role")]
    public Role Role { get; set; }

    public virtual Student? Student { get; set; }
    public virtual Counsellor? Counsellor { get; set; }
    public virtual AccessRight? AccessRight { get; set; }
}

public static class UserConverter
{
    public static Domain.Entities.User ToDomainUser(this User user)
    {
        var tgInfo = new TelegramInfo { TgUsername = user.TgName, TgId = user.TgId };
        FullName fullName = null;
        string city = null;
        string phone = null;
        string email = null;
        DateOnly? birthDate = new();
        if (user.Role == Role.Counsellor && user.Counsellor != null)
        {
            fullName = new FullName
            {
                Name = user.Counsellor.Name,
                Surname = user.Counsellor.Surname,
                Patronymic = user.Counsellor.Patronymic
            };
            city = user.Counsellor.City;
            phone = user.Counsellor.Phone;
            email = user.Counsellor.Email;
            birthDate = user.Counsellor.BirthDate;
        }
        else if (user.Role == Role.Child && user.Student != null)
        {
            fullName = new FullName
            {
                Name = user.Student.Name,
                Surname = user.Student.Surname,
                Patronymic = user.Student.Patronymic
            };
            city = user.Student.City;
            phone = user.Student.Phone;
            email = user.Student.Email;
            birthDate = user.Student.BirthDate;
        }
        return new Domain.Entities.User
        {
            Id = user.UserId,
            FullName = fullName,
            TelegramInfo = tgInfo,
            Email = email,
            PhoneNumber = phone,
            Birthday = birthDate,
            City = city,
            Role = user.Role,
            ChildInfo = user.Student?.ToChildInfo(),
            CounsellorInfo = user.Counsellor?.ToCounsellorInfo()
        };
    }
    
    public static User FromDomainUser(Domain.Entities.User user)
    {
        var dbUser = new User
        {
            UserId = user.Id,
            TgName = user.TelegramInfo.TgUsername,
            TgId = user.TelegramInfo.TgId,
            Role = user.Role
        };

        if (user.Role == Role.Child && user.ChildInfo != null)
        {
            dbUser.Student = new Student
            {
                UserId = user.Id,
                Name = user.FullName.Name,
                Surname = user.FullName.Surname,
                Patronymic = user.FullName.Patronymic,
                City = user.City,
                School = user.ChildInfo.EducationInfo.School,
                Email = user.Email,
                Phone = user.PhoneNumber,
                BirthDate = user.Birthday ?? default,
                CurrentClass = user.ChildInfo.EducationInfo.Class,
                CurrentGroup = user.ChildInfo.Group
            };
        }
        else if (user.Role == Role.Counsellor && user.CounsellorInfo != null)
        {
            dbUser.Counsellor = new Counsellor
            {
                UserId = user.Id,
                Name = user.FullName.Name,
                Surname = user.FullName.Surname,
                Patronymic = user.FullName.Patronymic,
                City = user.City,
                Email = user.Email,
                Phone = user.PhoneNumber,
                BirthDate = user.Birthday ?? default,
                CurrentGroup = user.CounsellorInfo.Group,
            };
        }

        return dbUser;
    }
}
