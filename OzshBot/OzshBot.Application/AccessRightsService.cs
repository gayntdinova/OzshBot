using OzshBot.Application.Interfaces;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application;

public class AccessRightsService: IAccessRightsService
{
    public Task<AccessRights> GetAccessRightsAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PromoteAccessRightsAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DemoteAccessRightsAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PromoteToCounsellor(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }
}