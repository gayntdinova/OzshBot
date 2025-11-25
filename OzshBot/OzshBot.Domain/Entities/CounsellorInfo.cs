using System.ComponentModel.DataAnnotations;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class CounsellorInfo
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required FullName FullName { get; set; }
    public required DateOnly Birthday { get; set; }
    public required string? City { get; set; }
    [Phone]
    public required string? PhoneNumber { get; set; }
    [EmailAddress]
    public required string? Email { get; set; }
    public required int? Group { get; set; }
    public required Session[] Sessions { get; set; }
}