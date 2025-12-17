using System.Text.RegularExpressions;
using OneOf.Types;
using OzshBot.Application.AppErrors;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = OzshBot.Domain.Entities.User;
using OzshBot.Application.Services.Interfaces;

namespace OzshBot.Bot;

public class UserRegistrator
{
    private readonly IUserRoleService userRoleService;
    public UserRegistrator(IUserRoleService userRoleService)
    {
        this.userRoleService = userRoleService;
    }
    public async Task<Role> LogInAndRegisterUserAsync(Message msg)
    {
        var tgInfo = new TelegramInfo { TgUsername = msg.From!.Username!, TgId = msg.From.Id };
        var userRole = await userRoleService.GetUserRoleByTgAsync(tgInfo);
        if (userRole == Role.Unknown)
            userRole = await Register(tgInfo, msg);
            Console.WriteLine("регистрация прошла");
        return userRole;
    }
    
    private async Task<Role> Register(TelegramInfo tgInfo, Message msg)
    {
        var phone = GetPhoneNumber(msg);
        Console.WriteLine(phone);
        if (phone == null) return Role.Unknown;
        phone = NormalizePhone(phone);
        var userRole = await userRoleService.ActivateUserByPhoneNumberAsync(phone, tgInfo);
        Console.WriteLine(userRole);
        return userRole;
    }
    
    
    public static string? GetPhoneNumber(Message msg)
    {
        if (msg.Contact == null)
            return null;
        
        return msg.Contact.PhoneNumber;
    }
    
    static string NormalizePhone(string input)
    {
        string digits = Regex.Replace(input, @"\D", "");
        
        if (digits.Length == 11 && (digits[0] == '7' || digits[1] == '8'))
        {
            digits = digits.Substring(1);
            return "+7" + digits;
        }
        throw new ArgumentException("invalid phone number");
    }
}