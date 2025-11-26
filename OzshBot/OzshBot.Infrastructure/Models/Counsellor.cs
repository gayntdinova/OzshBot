using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Infrastructure.Models;

[Table("counsellors")]
public class Counsellor
{
    [Key]
    [Column(name: "counsellor_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid CounsellorId { get; set; } = Guid.NewGuid();

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

    [Column(name: "birth_date")]
    public DateOnly BirthDate { get; set; } //TODO мне кажется можно и в домене сделать DateOnly?

    [Column(name: "current_group")]
    public int? CurrentGroup { get; set; }

    [Required]
    [Column(name: "email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [Column(name: "phone")]
    [Phone]
    public string? Phone { get; set; }
}


public static class CounsellorConverter
{
    public static Domain.Entities.CounsellorInfo ToCounsellorInfo(this Counsellor counsellor)
    {
        var fullName = new FullName
        {
            Name = counsellor.Name,
            Surname = counsellor.Surname,
            Patronymic = counsellor.Patronymic
        };
        var result = new Domain.Entities.CounsellorInfo
        {
            Id = counsellor.CounsellorId,
            FullName = fullName,
            Birthday = counsellor.BirthDate,
            City = counsellor.City,
            PhoneNumber = counsellor.Phone,
            Email = counsellor.Email,
            Group = counsellor.CurrentGroup,
            Sessions = []
        };
        return result;
    }

    public static Domain.Entities.User ToDomainUser(this Counsellor counsellor)
    {
        var tgInfo = new TelegramInfo { TgUsername = counsellor.User.TgName, TgId = counsellor.User.TgId };
        return new Domain.Entities.User
        {
            Id = counsellor.User.UserId,
            TelegramInfo = tgInfo,
            CounsellorInfo = counsellor.ToCounsellorInfo(),
            ChildInfo = counsellor.User.Student?.ToChildInfo(),
            Role = Domain.Enums.Role.Counsellor
        };
    }

    public static Counsellor? FromCounsellorInfo(CounsellorInfo? counsellorInfo)
    {
        if (counsellorInfo == null) return null;
        return new Counsellor
        {
            CounsellorId = counsellorInfo.Id,
            Name = counsellorInfo.FullName.Name,
            Surname = counsellorInfo.FullName.Surname,
            Patronymic = counsellorInfo.FullName.Patronymic,
            City = counsellorInfo.City,
            BirthDate = counsellorInfo.Birthday,
            CurrentGroup = counsellorInfo.Group,
            Email = counsellorInfo.Email,
            Phone = counsellorInfo.PhoneNumber
        };
    }
    
    public static Counsellor? FromCounsellorInfo (CounsellorInfo? counsellorInfo, Domain.Entities.User user)
    {
        if (counsellorInfo == null) return null;
        return new Counsellor
        {
            UserId = user.Id,
            CounsellorId = counsellorInfo.Id,
            Name = counsellorInfo.FullName.Name,
            Surname = counsellorInfo.FullName.Surname,
            Patronymic = counsellorInfo.FullName.Patronymic,
            City = counsellorInfo.City,
            BirthDate = counsellorInfo.Birthday,
            CurrentGroup = counsellorInfo.Group,
            Email = counsellorInfo.Email,
            Phone = counsellorInfo.PhoneNumber
        };
    }
}
