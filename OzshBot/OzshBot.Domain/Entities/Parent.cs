using OzshBot.Domain.ValueObjects;
namespace OzshBot.Domain.Entities;

public class Parent
{
    public Guid Id { get; set; }
    public required FullName FullName { get; set; }
    public required string PhoneNumber { get; set; }
}