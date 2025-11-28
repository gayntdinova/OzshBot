using System.Threading.Tasks;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;
using OzshBot.Infrastructure.Data;
using OzshBot.Infrastructure.Services;

namespace OzshBot.Infrastructure.DbTest;

class Program
{
    static async Task Main(string[] args)
    {
        var dbContext = AppDbContextFactory.CreateContext();
        var dbRepository = new DbRepository(dbContext);
    }
}

