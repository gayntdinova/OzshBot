using System.ComponentModel.DataAnnotations;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class CounsellorInfo
{
    public int? Group { get; set; }
    public Session[] Sessions { get; init; } = [];
}