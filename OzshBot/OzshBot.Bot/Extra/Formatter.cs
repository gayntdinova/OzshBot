using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;
using UserDomain = OzshBot.Domain.Entities.User;

namespace OzshBot.Bot.Extra;

public class Formatter: IFormatter
{
    public string FormatFullName(FullName fullName)
    {
        var result = string.Empty;
        if (fullName.Surname != null) result += $" {fullName.Surname}";
        if (fullName.Name != null) result += $" {fullName.Name}";
        if (fullName.Patronymic != null) result += $" {fullName.Patronymic}";
        return result;
    }

    public string FormatUser(UserDomain user, Role role)
    {
        var childInfo = user.ChildInfo;
        var counsellorInfo = user.CounsellorInfo;

        var answer = "";
        answer += $"{FormatFullName(user.FullName)}";

        answer += user.TelegramInfo == null ? "" : $"\n@{user.TelegramInfo.TgUsername}";

        answer += childInfo?.Group == null ? "" : $"\nГруппа: `{childInfo.Group}`";
        answer += counsellorInfo?.Group == null ? "" : $"\nГруппа: `{counsellorInfo.Group}`";

        answer += user.City == null ? "" : $"\nГород: `{user.City}`";

        answer += childInfo?.EducationInfo == null
            ? ""
            : $"\nШкола: `{childInfo.EducationInfo.School}`, {childInfo.EducationInfo.Class} класс";

        answer += user.Birthday == null ? "" : $"\n\nДата рождения: {user.Birthday}";

        if (role != Role.Counsellor) return FormatString(answer);
        answer += "\n";

        answer += user.Email == null ? "" : $"\nПочта: `{user.Email}`";

        answer += user.PhoneNumber == null ? "" : $"\nТелефон: `{user.PhoneNumber}`";

        if (childInfo != null && childInfo.ContactPeople.Count != 0)
            answer += "\n\n" +
                      "Родители:\n" +
                      string.Join("\n", childInfo.ContactPeople
                          .Select(parent =>
                              $" - {FormatFullName(parent.FullName)}\n" +
                              $"   `{parent.PhoneNumber}`"));
        return FormatString(answer);
    }

    public string FormatUsers(IEnumerable<UserDomain> users, string message)
    {
        var children = users.Where(user => user.Role == Role.Child);
        var counsellors = users.Where(user => user.Role == Role.Counsellor);
        var answer = $"{message}:\n\n";
        if (children.Count() != 0)
            answer += "Дети:\n" +
                      string.Join("\n", children
                          .Select(child =>
                              $" -`{FormatFullName(child.FullName)}`" +
                              (child.TelegramInfo == null ? "" : $" @{child.TelegramInfo.TgUsername}") +
                              (child.ChildInfo?.Group == null ? "" : $" группа {child.ChildInfo.Group}")
                          )) + "\n\n";
        if (counsellors.Count() != 0)
            answer += "Вожатые:\n" +
                      string.Join("\n", counsellors
                          .Select(counsellor =>
                              $" -`{FormatFullName(counsellor.FullName)}`" +
                              (counsellor.TelegramInfo == null ? "" : $" @{counsellor.TelegramInfo.TgUsername}") +
                              (counsellor.CounsellorInfo?.Group == null
                                  ? ""
                                  : $" группа {counsellor.CounsellorInfo.Group}")
                          ));
        return FormatString(answer);
    }

    public string FormatSessions(Session[] sessions, UserDomain user)
    {
        var answer = $"Смены, на которых был {user.FullName.Name}:\n" +
                     string.Join("\n", sessions
                         .Select(session =>
                             $" - `{session.SessionDates.StartDate.ToString("dd.MM.yyyy")} {session.SessionDates.EndDate.ToString("dd.MM.yyyy")}`"));
        return FormatString(answer);
    }

    public string FormatSessions(Session[] sessions)
    {
        var answer = $"Смены:\n" +
                     string.Join("\n", sessions
                         .Select(session =>
                             $" - `{session.SessionDates.StartDate.ToString("dd.MM.yyyy")} {session.SessionDates.EndDate.ToString("dd.MM.yyyy")}`"));
        return FormatString(answer);
    }

    public string FormatString(string text)
    {
        return text.Replace(".", "\\.").Replace("-", "\\-").Replace("+", "\\+").Replace("*", "\\*")
            .Replace("(", "\\(").Replace(")", "\\)").Replace("_", "\\_");
    }
}