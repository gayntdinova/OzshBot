using System.ComponentModel.DataAnnotations;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class Counsellor
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required FullName FullName { get; set; }
    public required DateOnly Birthday { get; set; }
    public required string? Town { get; set; }
    public required TelegramInfo TelegramInfo { get; set; }
    public AccessRights AccessRights { get; set; } = AccessRights.Read;
    [Phone]
    public required string? PhoneNumber { get; set; }
    [EmailAddress]
    public required string? Email { get; set; }
    public required int? Group { get; set; }
    public required List<Session> Sessions { get; set; }
}