using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Data;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Services;

public class UserService(AppDbContext context)
{
    private readonly AppDbContext context = context;

    public async Task<User?> GetUserByTgIdAsync(long tgId)
    {
        return await context.Users
            .Include(u => u.Person)
            .Include(u => u.AccessRight)
            .FirstOrDefaultAsync(u => u.TgId == tgId);
    }

    public async Task<User?> GetUserByTgNameAsync(string tgName)
    {
        return await context.Users
            .Include(u => u.Person)
            .Include(u => u.AccessRight)
            .FirstOrDefaultAsync(u => u.TgName == tgName);
    }

    public async Task<User> CreateUserAsync(string telegramName, long? telegramId = null)
    {
        var user = new User
        {
            TgName = telegramName,
            TgId = telegramId ?? 0
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        var accessRight = new AccessRight
        {
            UserId = user.UserId,
            Rights = Access.Read
        };
        context.AccessRights.Add(accessRight);
        await context.SaveChangesAsync();
        
        return user;
    }

    public async Task<bool> UserExistsAsync(string telegramName)
    {
        return await context.Users.AnyAsync(u => u.TgName == telegramName);
    }
}