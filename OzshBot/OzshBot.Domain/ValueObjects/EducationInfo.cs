namespace OzshBot.Domain.ValueObjects;

public record EducationInfo
{
    public required int Class { get; init; }
    public required string School { get; init; }

    public override string ToString()
    {
        return $"EducationInfo({School}, {Class})";
    }
}