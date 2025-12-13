namespace OzshBot.Domain.ValueObjects;

public class SessionDates
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public SessionDates(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}