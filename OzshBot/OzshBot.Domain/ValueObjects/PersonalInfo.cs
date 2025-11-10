using System.ComponentModel.DataAnnotations;

namespace OzshBot.Domain.ValueObjects;

public class PersonalInfo
{
    public required FullName FullName { get; set; }
    public required DateOnly Birthday { get; set; }
    public required string? Town { get; set; }
    [Phone]
    public required string? PhoneNumber { get; set; }
    [EmailAddress]
    public required string? Email { get; set; }

}