using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class CounsellorDto
{
    public TelegramInfo TelegramInfo { get; set; }
    public CounsellorInfo CounsellorInfo { get; set; }
}