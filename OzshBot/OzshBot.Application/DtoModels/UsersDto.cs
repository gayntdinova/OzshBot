using OzshBot.Domain.Entities;

namespace OzshBot.Application.DtoModels;

public class UsersDto
{
    public Counsellor[]? Counsellor { get; init; }
    public Child[]? Child { get; init; }
}