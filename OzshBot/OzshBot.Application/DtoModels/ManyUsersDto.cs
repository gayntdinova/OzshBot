using OzshBot.Domain.Entities;

namespace OzshBot.Application.DtoModels;

public class ManyUsersDto
{
    public Counsellor[]? Counsellor { get; init; }
    public Child[]? Child { get; init; }
}