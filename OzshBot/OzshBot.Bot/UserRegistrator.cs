using System.Text.RegularExpressions;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Telegram.Bot.Types;
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
        {
            userRole = await Register(tgInfo, msg);
            Console.WriteLine("регистрация прошла");
        }
            
        return userRole;
    }
    
    private async Task<Role> Register(TelegramInfo tgInfo, Message msg)
    {
        var phone = msg.Contact?.PhoneNumber;
        Console.WriteLine(phone);
        if (phone == null) return Role.Unknown;
        phone = NormalizePhone(phone);
        var userRole = await userRoleService.ActivateUserByPhoneNumberAsync(phone, tgInfo);
        Console.WriteLine(userRole);
        return userRole;
    }
    
    private static string NormalizePhone(string input)
    {
        var digits = Regex.Replace(input, @"\D", "");
        
        if (digits.Length == 11 && (digits[0] == '7' || digits[1] == '8'))
        {
            digits = digits.Substring(1);
            return "+7" + digits;
        }
        throw new ArgumentException("invalid phone number");
    }
}