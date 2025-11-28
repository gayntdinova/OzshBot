using System.ComponentModel.DataAnnotations;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public interface IUserDtoModel
{
    public Guid Id { get; init; }
    public FullName FullName { get; set; }
    public TelegramInfo TelegramInfo { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? City { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    [EmailAddress]
    public string? Email { get; set; }

    public User ToUser();
}