using FluentResults;
using OzshBot.Application.AppErrors;
namespace OzshBot.Bot;

public static class ErrorExplanationManager
{
    private static readonly Dictionary<Type, Func<IError, string>> ReplyDict = new()
    {
        {
            typeof(IncorrectRowError),
            error =>
            {
                var e = (IncorrectRowError)error;
                return $"Некорректный формат строки {e.Row}";
            }
        },
        {
            typeof(IncorrectUrlError),
            _ => "Некорректный url"
        },
        {
            typeof(SessionAlreadyExistsError),
            _ => "Сессия уже существует"
        },
        {
            typeof(SessionIntersectError),
            _ => "Нельзя чтобы даты сессий пересекались"
        },
        {
            typeof(SessionNotFoundError),
            _ => "Сессий нету, ну вообще"
        },
        {
            typeof(UserAlreadyExistsError),
            _ => "Пользователь уже существует"
        },
        {
            typeof(UserNotFoundError),
            _ => "Пользователь не найден"
        }
    };

    public static string GetExplanation(this IError error)
    {
        if (ReplyDict.TryGetValue(error.GetType(), out var factory))
            return factory(error);

        return "Неизвестная ошибка";
    }
}