using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class RoleService: IRoleService
{
    public async Task<Role> GetUserRole(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> PromoteToCounsellor(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }
}