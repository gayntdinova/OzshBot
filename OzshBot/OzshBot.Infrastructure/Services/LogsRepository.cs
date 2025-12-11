using OzshBot.Infrastructure.Data;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Services;

public class LogsRepository(LogsDbContext dbContext) : ISearchLogger
{
    private readonly LogsDbContext context = dbContext;

    public async Task Log(long tgId, DateOnly date, bool success)
    {
        var newLog = new Log { TgId = tgId, Date = date, Success = success };
        await context.AddAsync(newLog);
    }
}