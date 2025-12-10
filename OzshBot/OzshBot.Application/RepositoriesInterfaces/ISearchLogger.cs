namespace OzshBot.Application.RepositoriesInterfaces;

public interface ISearchLogger
{
    public void Log(long tgId, DateOnly date, bool success);
}