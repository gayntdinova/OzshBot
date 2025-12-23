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
    public DateOnly? BirthDate { get; set; }

    [Column(name: "current_group")]
    public int? CurrentGroup { get; set; }

    [Column(name: "email")]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [Column(name: "phone")]
    [Phone]
    public string? Phone { get; set; }

    public virtual List<CounsellorSession>? SessionRelations { get; set; }
    [NotMapped]
    public List<Session> Sessions => SessionRelations?.Select(r => r.Session).ToList() ?? [];
}


public static class CounsellorConverter
{
    public static CounsellorInfo ToCounsellorInfo(this Counsellor counsellor)
    {
        return new CounsellorInfo
        {
            Group = counsellor.CurrentGroup,
            Sessions = counsellor.Sessions.Select(s => s.ToDomainSession()).ToHashSet()
        };
    }

    public static void UpdateFromCounsellorInfo(this Counsellor counsellor, CounsellorInfo counsellorInfo)
    {
        counsellor.CurrentGroup = counsellorInfo.Group;
    }
}
