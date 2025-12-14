namespace OzshBot.Domain.ValueObjects;

public record SessionDates
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public SessionDates(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}