using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class UserDto
{
    public Counsellor? Counsellor { get; init; }
    public Child? Child { get; init; }
    public Role Role { get; init; }
}