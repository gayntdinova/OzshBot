namespace OzshBot.Infrastructure.Interfaces;

public interface IBdCounsellor
{
    public Guid PersonId { get; set; }
    public Guid UserId { get; set; }

    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Patronymic { get; set; }

    public DateOnly? BirthDate { get; set; }

    public int CurrentGroup { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }
}