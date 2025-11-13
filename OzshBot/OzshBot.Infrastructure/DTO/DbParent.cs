using System.ComponentModel.DataAnnotations;

namespace OzshBot.Infrastructure.DTO;

public class DbParent
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? Patronymic { get; set; }

    [Phone]
    public string? Phone { get; set; }
}