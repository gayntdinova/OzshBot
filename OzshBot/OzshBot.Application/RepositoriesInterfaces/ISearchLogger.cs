namespace OzshBot.Application.RepositoriesInterfaces;

public interface ISearchLogger
{
    Task Log(long tgId, DateOnly date, bool success);
}