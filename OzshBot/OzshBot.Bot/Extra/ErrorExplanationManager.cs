using FluentResults;
using OzshBot.Application.AppErrors;

namespace OzshBot.Bot.Extra;

public static class ErrorExplanationManager
{
    private static readonly Dictionary<Type, Func<IError, string>> ReplyDict = new()
    {
        {
            typeof(InvalidRowError),
            error =>
            {
                var e = (InvalidRowError)error;
                return $"Некорректный формат строки {e.Row}";
            }
        },
        {
            typeof(InvalidUrlError),
            _ => "Некорректный url"
        },
        {
            typeof(InvalidDataError),
            error =>
            {
                var e = (InvalidDataError)error;
                return $"Некорректные данные: {e.Message}";
            }
        },
        {
            typeof(InvalidTableFormatError),
            _ => "Некорректный формат таблицы"
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
        return ReplyDict.TryGetValue(error.GetType(), out var func)
            ? func(error)
            : "Неизвестная ошибка";
    }
}