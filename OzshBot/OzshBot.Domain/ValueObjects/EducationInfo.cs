namespace OzshBot.Domain.ValueObjects;

public class EducationInfo
{
    public required int Class { get; set; }
    public required string School { get; set; }

    public override string ToString()
    {
        return $"EducationInfo({School}, {Class})";
    }
}