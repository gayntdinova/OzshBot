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
    public long TgId { get; set; }

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
        return new Domain.Entities.User
        {
            Id = user.UserId,
            TelegramInfo = tgInfo,
            Role = user.Role,
            ChildInfo = user.Student?.ToChildInfo(),
            CounsellorInfo = user.Counsellor?.ToCounsellorInfo()
        };
    }
}
