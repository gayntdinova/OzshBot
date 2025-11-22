using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Infrastructure.Models;

[Table("parents")]
public class Parent
{
    [Key]
    [Column(name: "parent_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ParentId { get; set; }

    [Required]
    [Column(name: "name")]
    public required string Name { get; set; }
    [Required]
    [Column(name: "surname")]
    public required string Surname { get; set; }
    [Column(name: "patronymic")]
    public string? Patronymic { get; set; }

    [Required]
    [Column(name: "phone")]
    [Phone]
    public required string Phone { get; set; }

    public virtual List<ChildParent>? Relations { get; set; }
    [NotMapped]
    public List<Student>? Children => Relations?.Select(r => r.Child).ToList();
}

public static class ParentConverter
{
    public static Domain.Entities.ParentInfo ToParentInfo(this Parent parent)
    {
        var fullName = new FullName
        {
            Name = parent.Name,
            Surname = parent.Surname,
            Patronymic = parent.Patronymic
        };
        var result = new Domain.Entities.ParentInfo
        {
            Id = parent.ParentId,
            FullName = fullName,
            PhoneNumber = parent.Phone
        };
        return result;
    }
}
