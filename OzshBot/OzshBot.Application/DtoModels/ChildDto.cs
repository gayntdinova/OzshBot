using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class ChildDto
{
    public TelegramInfo TelegramInfo { get; set; }
    public ChildInfo ChildInfo { get; set; }
}