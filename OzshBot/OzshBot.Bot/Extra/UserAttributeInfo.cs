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
namespace OzshBot.Bot;

public enum UserAttribute
{
    Role,
    SessionAdd,
    SessionEdit,
    PhoneNumber,
    TgUsername,
    FullName,
    Email,
    Birthday,
    City,
    Class,
    School,
    Group,
}

public class UserAttributeInfo
{

    public readonly string Name;
    public readonly string WritingInfo;
    public readonly Func<string,Task<bool>> CorrectFormateFunction;
    public readonly Action<UserDomain,string> FillingAction;
    public readonly Func<UserDomain,Task<ReplyKeyboardMarkup>>? KeyboardMarkup;

    public UserAttributeInfo(string name, string writingInfo, Func<string,Task<bool>> correctFormateFunction, Action<UserDomain,string> fillingAction,Func<UserDomain,Task<ReplyKeyboardMarkup>>? keyboardMarkup = null)
    {
        Name = name;
        WritingInfo = writingInfo;
        CorrectFormateFunction = correctFormateFunction;
        FillingAction = fillingAction;
        KeyboardMarkup = keyboardMarkup;
    }
}

public static class UserAttributesInfoManager
{
    private static Dictionary<UserAttribute,UserAttributeInfo>? UserAttributeInfoDict;
    private static Dictionary<Role,UserAttribute[]> RolesAttributesDict = new()
    {
        {
            Role.Unknown,
            new UserAttribute[0]
        },
        {
            Role.Child,
            new[]{
                UserAttribute.SessionAdd,
                UserAttribute.SessionEdit,
                UserAttribute.Role,
                UserAttribute.PhoneNumber,
                UserAttribute.TgUsername,
                UserAttribute.FullName,
                UserAttribute.Email,
                UserAttribute.Birthday,
                UserAttribute.City,
                UserAttribute.Group,
                UserAttribute.School,
                UserAttribute.Class}
        },
        {
            Role.Counsellor,
            new[]{
                UserAttribute.SessionAdd,
                UserAttribute.SessionEdit,
                UserAttribute.Role,
                UserAttribute.PhoneNumber,
                UserAttribute.TgUsername,
                UserAttribute.FullName,
                UserAttribute.Email,
                UserAttribute.Birthday,
                UserAttribute.City,
                UserAttribute.Group}
        }
    };

    public readonly static UserAttribute[] EditableAttributes = new[]{
        UserAttribute.PhoneNumber,
        UserAttribute.SessionEdit,
        UserAttribute.TgUsername,
        UserAttribute.FullName,
        UserAttribute.Email,
        UserAttribute.Birthday,
        UserAttribute.City,
        UserAttribute.Group,
        UserAttribute.School,
        UserAttribute.Class};

    public readonly static UserAttribute[] AddableAttributes = new[]{
        UserAttribute.Role,
        UserAttribute.SessionAdd,
        UserAttribute.PhoneNumber,
        UserAttribute.TgUsername,
        UserAttribute.FullName,
        UserAttribute.Email,
        UserAttribute.Birthday,
        UserAttribute.City,
        UserAttribute.Group,
        UserAttribute.School,
        UserAttribute.Class};

    public static void Initialize(ISessionService sessionService)
    {
        UserAttributeInfoDict = new()
        {
            {
                UserAttribute.SessionAdd,
                new UserAttributeInfo(
                    "Смена",
                    "Выберите в какую смену добавить нового пользователя",
                    async str=>
                    {
                        if (Regex.IsMatch(str,@"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}) (0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                        {
                            Console.WriteLine("тут проходит");
                            var splitted = str.Split(" ");
                            var startDate = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                            var endDate = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");

                            var sessions = await sessionService.GetAllSessionsAsync();

                            if (sessions.Any(session=>session.SessionDates.StartDate==startDate && session.SessionDates.EndDate == endDate))
                                return true;

                        }
                        return false;
                    },
                    async (UserDomain user, string message) =>
                    {
                        var splitted = message.Split(" ");
                        var startDate = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                        var endDate = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");

                        var sessions = await sessionService.GetAllSessionsAsync();

                        var session = sessions.First(session=>session.SessionDates.StartDate==startDate && session.SessionDates.EndDate == endDate);
                        
                        if(user.ChildInfo!=null)
                            user.ChildInfo.Sessions.Add(session);
                        if(user.CounsellorInfo!=null)
                            user.CounsellorInfo.Sessions.Add(session);
                    },
                    async (user)=> new ReplyKeyboardMarkup((await sessionService.GetAllSessionsAsync())
                        .Select(session=>new KeyboardButton[]{new KeyboardButton($"{session.SessionDates.StartDate.ToString("dd.MM.yyyy")} {session.SessionDates.EndDate.ToString("dd.MM.yyyy")}")}))
                    {ResizeKeyboard = true})
            },
            {
                UserAttribute.SessionEdit,
                new UserAttributeInfo(
                    "Смены",
                    "Выберите как и какую смену менять, для этого напишите remove или add и даты смены(или просто выберите из списка)",
                    async str=>
                    {
                        if (Regex.IsMatch(str,@"^(add|remove)\s(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})\s(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                        {
                            var splitted = str.Split(" ");
                            var startDate = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");
                            var endDate = DateOnly.ParseExact(splitted[2], "dd.MM.yyyy");

                            var sessions = await sessionService.GetAllSessionsAsync();

                            if (sessions.Any(session=>session.SessionDates.StartDate==startDate && session.SessionDates.EndDate == endDate))
                                return true;

                        }
                        return false;
                    },
                    async (UserDomain user, string message) =>
                    {
                        var splitted = message.Split(" ");
                        var command = splitted[0];
                        var startDate = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");
                        var endDate = DateOnly.ParseExact(splitted[2], "dd.MM.yyyy");

                        var sessions = await sessionService.GetAllSessionsAsync();

                        var session = sessions.First(session=>session.SessionDates.StartDate==startDate && session.SessionDates.EndDate == endDate);
                        
                        if(command == "add")
                        {
                            if(user.ChildInfo!=null)
                                user.ChildInfo.Sessions.Add(session);
                            if(user.CounsellorInfo!=null)
                                user.CounsellorInfo.Sessions.Add(session);
                        }
                        else
                        {
                            if(user.ChildInfo!=null)
                                user.ChildInfo.Sessions.Remove(session);
                            if(user.CounsellorInfo!=null)
                                user.CounsellorInfo.Sessions.Remove(session);
                        }
                    },
                    async (user)=>
                    {
                        var userSessions =new HashSet<Session>();
                        if (user.ChildInfo!=null)
                            userSessions = user.ChildInfo.Sessions;
                        if (user.CounsellorInfo!=null)
                            userSessions = user.CounsellorInfo.Sessions;

                        var toAdd = userSessions
                            .Select(session=>new KeyboardButton[]{new KeyboardButton($"remove {session.SessionDates.StartDate.ToString("dd.MM.yyyy")} {session.SessionDates.EndDate.ToString("dd.MM.yyyy")}")});
                        var toDelete = (await sessionService.GetAllSessionsAsync())
                            .Where(ses=>!userSessions.Any(sess=>sess.Id==ses.Id))
                            .Select(session=>new KeyboardButton[]{new KeyboardButton($"add {session.SessionDates.StartDate.ToString("dd.MM.yyyy")} {session.SessionDates.EndDate.ToString("dd.MM.yyyy")}")});
                        
                        return new ReplyKeyboardMarkup(toAdd.Concat(toDelete))
                        {ResizeKeyboard = true};
                    })
            },
            {
                UserAttribute.Role,
                new UserAttributeInfo(
                    "Роль",
                    "Напишите какой роли человека вы хотите создать: `ребёнок` или `вожатый`",
                    async str=>Regex.IsMatch(str,@"^(ребёнок|вожатый)$"),
                    (UserDomain user, string message) =>
                    {
                        if(message == "ребёнок")
                        {
                            user.Role = Role.Child;
                            user.ChildInfo = new();
                            user.ChildInfo.EducationInfo = new EducationInfo
                            {
                                School = "",
                                Class = 0
                            };
                        }
                        else
                        {
                            user.Role = Role.Counsellor;
                            user.CounsellorInfo = new();
                        }
                    })
            },
            {
                UserAttribute.PhoneNumber,
                new UserAttributeInfo(
                    "Телефон",
                    "Введите номер телефона(корректный формат: +7**********)",
                    async str=>Regex.IsMatch(str,@"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"),
                    (UserDomain user, string message) =>
                    {
                        user.PhoneNumber = message;
                    })
            },
            {
                UserAttribute.FullName,
                new UserAttributeInfo(
                    "ФИО",
                    "Введите полное имя полностью (корректный формат: Фамилия Имя Отчество или Фамилия Имя)",
                    async str=>Regex.IsMatch(str,@"^[А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+( [А-ЯЁ][а-яё]+)?$"),
                    (UserDomain user, string message) =>
                    {
                        var splittedMessage = message.Split(" ");
                        user.FullName = new(splittedMessage[0],splittedMessage[1],splittedMessage.Length==2?null:splittedMessage[2]);
                    })
            },
            {
                UserAttribute.TgUsername,
                new UserAttributeInfo(
                    "Телеграм",
                    "Введите юзернейм телеграма(корректный формат: @username)",
                    async str=>Regex.IsMatch(str,@"^@[A-Za-z0-9_]+$"),
                    (user, message) =>
                    {
                        user.TelegramInfo = new TelegramInfo
                        {
                            TgUsername = message.Substring(1)
                        };
                    })
            },
            {
                UserAttribute.Email,
                new UserAttributeInfo(
                    "Email",
                    "Введите email(корректный формат: чтото@чтото.чтото) или _",
                    async str=>Regex.IsMatch(str,@"^(_|[\w\.\-]+@[\w\-]+\.[A-Za-z]{2,})$"),
                    (user, message) =>
                    {
                        user.Email = message == "_" ? null : message;
                    })
            },
            {
                UserAttribute.Birthday,
                new UserAttributeInfo(
                    "День рождения",
                    "Введите день рождения(dd.MM.yyyy) или _",
                    async str=>Regex.IsMatch(str,@"^(_|(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}))$"),
                    (user, message) =>
                    {
                        user.Birthday = message == "_"
                            ? null
                            : DateOnly.ParseExact(message, "dd.MM.yyyy");
                    })
            },
            {
                UserAttribute.City,
                new UserAttributeInfo(
                    "Город",
                    "Введите название города или _",
                    async str=>Regex.IsMatch(str,@".+|_"),
                    (user, message) =>
                    {
                        user.City = message == "_" ? null : message;
                    })
            },
            {
                UserAttribute.Group,
                new UserAttributeInfo(
                    "Группа",
                    "Введите номер группы или _",
                    async str=>Regex.IsMatch(str,@"^(_|\d+)$"),
                    (user, message) =>
                    {
                        int? group = message == "_" ? null : int.Parse(message);

                        if (user.ChildInfo != null)
                            user.ChildInfo.Group = group;

                        if (user.CounsellorInfo != null)
                            user.CounsellorInfo.Group = group;
                    })
            },
            {
                UserAttribute.School,
                new UserAttributeInfo(
                    "Школа",
                    "Введите название школы",
                    async str=>Regex.IsMatch(str,@".+"),
                    (user, message) =>
                    {
                        user.ChildInfo!.EducationInfo = new EducationInfo(){Class = user.ChildInfo!.EducationInfo!.Class,School = message};
                    })
            },
            {
                UserAttribute.Class,
                new UserAttributeInfo(
                    "Класс",
                    "Введите номер класса",
                    async str=>Regex.IsMatch(str,@"^(?:[1-9]|30|11)$"),
                    (user, message) =>
                    {
                        user.ChildInfo!.EducationInfo = new EducationInfo(){Class = int.Parse(message),School = user.ChildInfo!.EducationInfo!.School};
                    })
            }
        };
    }

    public static UserAttributeInfo GetInfo(this UserAttribute attribute)
    {
        if (UserAttributeInfoDict == null)
            throw new InvalidOperationException("не инициализовано");
        return UserAttributeInfoDict[attribute];
    }

    public static bool ImplementsAttribute(this Role role, UserAttribute attribute)
        =>RolesAttributesDict[role].Contains(attribute);
}