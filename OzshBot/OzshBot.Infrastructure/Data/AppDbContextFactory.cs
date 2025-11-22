using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Models;
using OzshBot.Infrastructure.Enums;
using OzshBot.Domain.Enums;

namespace OzshBot.Infrastructure.Data
{
    public static class AppDbContextFactory
    {
        public static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(
                    "Host=localhost;Database=ozsh;Username=postgres;Password=postgres;Port=5433",
                    o => o.MapEnum<Season>("season").MapEnum<Role>("role").MapEnum<Access>("access"))
                .Options;
            return new AppDbContext(options);
        }
    }
}