using OzshBot.Infrastructure.Interfaces;

namespace OzshBot.Infrastructure.Services;

public class ResultDTO
{
    public List<IDbPerson> Students;
    public List<IDbPerson> Counsellors;
}