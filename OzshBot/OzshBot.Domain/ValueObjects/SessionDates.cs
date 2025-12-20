namespace OzshBot.Domain.ValueObjects;

public record SessionDates
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }

    public SessionDates(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static bool Validate(SessionDates sessionDates)
    {
        return sessionDates.StartDate <= sessionDates.EndDate;
    }
}