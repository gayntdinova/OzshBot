using System.ComponentModel.DataAnnotations;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.ValueObjects;

public class CounsellorInfo
{
    public int? Group { get; set; }
    public Session[] Sessions { get; init; } = [];
}