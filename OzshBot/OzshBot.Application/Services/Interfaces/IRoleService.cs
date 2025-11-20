using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IRoleService
{
    public Task<Role> GetUserRole(TelegramInfo telegramInfo);
    public Task<bool> PromoteToCounsellor(TelegramInfo telegramInfo);
 }