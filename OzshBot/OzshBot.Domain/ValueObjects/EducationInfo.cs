namespace OzshBot.Domain.ValueObjects;

public record EducationInfo
{
    public required int Class { get; init; }

    public required string School
    {
        get => school;
        init => school = value?.ToLower();
    }
    private readonly string school;
}