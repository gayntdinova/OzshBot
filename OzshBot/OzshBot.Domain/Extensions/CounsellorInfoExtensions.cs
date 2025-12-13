using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Extensions;

public static class CounsellorInfoExtensions
{
    public static CounsellorInfo Clone(this CounsellorInfo childInfo)
    {
        return new CounsellorInfo
        {
            Group = childInfo.Group,
            Sessions = childInfo.Sessions.ToHashSet(),
        };
    }
}