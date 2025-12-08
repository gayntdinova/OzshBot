using System.ComponentModel.DataAnnotations;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public abstract class UserDtoModel
{
    public Guid Id { get; init; }
    public FullName FullName { get; set; }
    public TelegramInfo? TelegramInfo { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? City { get; set; }
    [Phone]
    public required string PhoneNumber { get; set; }
    [EmailAddress]
    public string? Email { get; set; }

    public abstract User ToUser();
}