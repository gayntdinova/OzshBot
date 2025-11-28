using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public interface Dto
{
    public TelegramInfo TelegramInfo { get; set; }
    public User ToUser();
}