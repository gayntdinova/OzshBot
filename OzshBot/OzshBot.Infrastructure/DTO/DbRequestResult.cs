using OzshBot.Domain.Entities;

namespace OzshBot.Infrastructure.DTO;

public class DbRequestResult
{
    public List<ChildInfo> Children;
    public List<CounsellorInfo> Counsellors;
}
