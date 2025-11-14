using OzshBot.Application.Interfaces;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Implementations;

public class AccessRightsService: IAccessRightsService
{
    public async Task<AccessRights> GetAccessRightsAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> PromoteAccessRightsAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DemoteAccessRightsAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> PromoteToCounsellor(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }
}