using OzshBot.Infrastructure.Enums;

namespace OzshBot.Infrastructure.Interfaces;

public interface IDbPerson
{
    public Guid PersonId { get; set; }
    public Guid UserId { get; set; }

    public Role? Role { get; set; }

    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Patronymic { get; set; }

    public string? City { get; set; }
    public string? School { get; set; }

    public DateOnly? BirthDate { get; set; }

    public int Class { get; set; }
    public int CurrentClass { get; set; }
    public int CurrentGroup { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }


    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }
}