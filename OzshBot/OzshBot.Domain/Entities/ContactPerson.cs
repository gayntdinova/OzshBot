using OzshBot.Domain.ValueObjects;
namespace OzshBot.Domain.Entities;

public class ContactPerson
{
    public Guid Id { get; init; }
    public FullName FullName { get; set; }
    public string? PhoneNumber { get; set; }
}