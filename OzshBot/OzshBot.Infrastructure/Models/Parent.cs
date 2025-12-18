using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Domain.Entities;
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
    public static ContactPerson ToContactPerson(this Parent parent)
    {
        var fullName = new FullName
        (
            name: parent.Name,
            surname: parent.Surname,
            patronymic: parent.Patronymic
        );
        var result = new ContactPerson
        {
            Id = parent.ParentId,
            FullName = fullName,
            PhoneNumber = parent.Phone
        };
        return result;
    }

    public static Parent FromParentInfo(ContactPerson parentInfo)
    {
        return new Parent
        {
            ParentId = parentInfo.Id,
            Name = parentInfo.FullName.Name,
            Surname = parentInfo.FullName.Surname,
            Patronymic = parentInfo.FullName.Patronymic,
            Phone = parentInfo.PhoneNumber
        };
    }
}
