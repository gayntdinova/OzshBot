namespace OzshBot.Domain.ValueObjects;

public class EducationInfo (string school, int currentClass)
{
    public required int Class { get; set; } = currentClass;
    public required string School { get; set; } = school;

    public override string ToString()
    {
        return $"EducationInfo({School}, {Class})";
    }
}