using System.ComponentModel.DataAnnotations;
using OzshBot.Domain.ValueObjects;
namespace OzshBot.Domain.Entities;

public class Child
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required FullName FullName { get; set; }
    public required DateOnly Birthday { get; set; }
    public required string Town { get; set; }
    [Phone]
    public required string PhoneNumber { get; set; }
    [EmailAddress]
    public required string Email { get; set; }
    public required EducationInfo EducationInfo { get; set; }
    public required int? Group { get; set; }
    public required List<Session> Sessions { get; set; }
    public required List<ContactPerson>? Parents { get; set; }
    public required TelegramBotUser TelegramBotUser { get; set; }
}