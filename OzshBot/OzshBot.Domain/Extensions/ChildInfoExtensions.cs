using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Extensions;

public static class ChildInfoExtensions
{
    public static ChildInfo Clone(this ChildInfo childInfo)
    {
        return new ChildInfo
        {
            EducationInfo = childInfo.EducationInfo != null
                ? new EducationInfo
                {
                    Class = childInfo.EducationInfo.Class,
                    School = childInfo.EducationInfo.School,
                }
                : null,
            Group = childInfo.Group,
            Sessions = childInfo.Sessions.ToHashSet(),
            ContactPeople = childInfo.ContactPeople.ToHashSet(),
        };
    }
}