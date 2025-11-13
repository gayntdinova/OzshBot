using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Interfaces;

public interface IAccessRightsService
{
    public Task<AccessRights> GetAccessRightsAsync(TelegramInfo telegramInfo);
    public Task<bool> PromoteAccessRightsAsync(TelegramInfo telegramInfo);
    public Task<bool> DemoteAccessRightsAsync(TelegramInfo telegramInfo);
    public Task<bool> PromoteToCounsellor(TelegramInfo telegramInfo);
 }