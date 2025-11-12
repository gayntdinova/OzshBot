using OzshBot.Infrastructure.Interfaces;

namespace OzshBot.Infrastructure.Services;

public class ResultDTO
{
    public List<IDbStudent> Students;
    public List<IBdCounsellor> Counsellors;
}