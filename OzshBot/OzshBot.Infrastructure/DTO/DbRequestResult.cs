using OzshBot.Domain.Entities;

namespace OzshBot.Infrastructure.DTO;

public class DbRequestResult
{
    public List<Child> Students;
    public List<Counsellor> Counsellors;
}
