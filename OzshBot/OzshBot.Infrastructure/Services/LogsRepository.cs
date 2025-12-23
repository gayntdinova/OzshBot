using OzshBot.Infrastructure.Data;
using OzshBot.Application.ToolsInterfaces;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Services;

public class LogsRepository(LogsDbContext dbContext) : ILogger
{
    private readonly LogsDbContext context = dbContext;

    public async Task Log(long tgId, bool success)
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        Console.WriteLine($"log: {tgId}, {date}, {(success? "success" : "fail")}");
        var newLog = new Log { TgId = tgId, Date = date, Success = success };
        await context.AddAsync(newLog);
        await context.SaveChangesAsync();
    }
}