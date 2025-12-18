using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Ninject;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Formats.Asn1;
using UserDomain = OzshBot.Domain.Entities.User;
using UserTg = Telegram.Bot.Types.User;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using OzshBot.Application.Services;
using System.Data;
using System.Text.RegularExpressions;
using FluentResults;
using System.Net.Http.Headers;
using System.Windows.Input;
using System.ComponentModel.DataAnnotations;
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