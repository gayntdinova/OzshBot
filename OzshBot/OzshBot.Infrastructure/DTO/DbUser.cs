using OzshBot.Infrastructure.Enums;

namespace OzshBot.Infrastructure.DTO;

public class DbUser
{
    public Guid UserId { get; set; }
    public required string TgName { get; set; }
    public long TgId { get; set; }

    public Access Rights { get; set; }
}