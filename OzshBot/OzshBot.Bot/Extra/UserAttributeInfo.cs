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
    public readonly string RegularExpression;
    public readonly Action<UserDomain,string> FillingAction;

    public UserAttributeInfo(string name, string writingInfo, string regularExpression, Action<UserDomain,string> fillingAction)
    {
        Name = name;
        WritingInfo = writingInfo;
        RegularExpression = regularExpression;
        FillingAction = fillingAction;
    }
}

public static class UserAttributesExtention
{
    private static Dictionary<UserAttribute,UserAttributeInfo> UserAttributeInfoDict = new()
    {
        {
            UserAttribute.Role,
            new UserAttributeInfo(
                "Роль",
                "Напишите какой роли человека вы хотите создать: `ребёнок` или `вожатый`",
                @"^(ребёнок|вожатый)$",
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
                @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$",
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
                @"^[А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+( [А-ЯЁ][а-яё]+)?$",
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
                @"^@[A-Za-z0-9_]+$",
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
                @"^(_|[\w\.\-]+@[\w\-]+\.[A-Za-z]{2,})$",
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
                @"^(_|(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}))$",
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
                @".+|_",
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
                @"^(_|\d+)$",
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
                @".+",
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
                @"^(?:[1-9]|10|11)$",
                (user, message) =>
                {
                    user.ChildInfo!.EducationInfo = new EducationInfo(){Class = int.Parse(message),School = user.ChildInfo!.EducationInfo!.School};
                })
        }
    };
    private static Dictionary<Role,UserAttribute[]> RolesAttributesDict = new()
    {
        {
            Role.Unknown,
            new UserAttribute[0]
        },
        {
            Role.Child,
            new[]{
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

    private static UserAttribute[] EditableAttributes = new[]{
        UserAttribute.PhoneNumber,
        UserAttribute.TgUsername,
        UserAttribute.FullName,
        UserAttribute.Email,
        UserAttribute.Birthday,
        UserAttribute.City,
        UserAttribute.Group,
        UserAttribute.School,
        UserAttribute.Class};


    public static UserAttributeInfo GetInfo(this UserAttribute attribute)
        => UserAttributeInfoDict[attribute];

    public static bool ImplementsAttribute(this Role role, UserAttribute attribute)
        =>RolesAttributesDict[role].Contains(attribute);

    public static bool IsEditable(this UserAttribute attribute)
        => EditableAttributes.Contains(attribute);
}