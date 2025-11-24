using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Data;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Domain.ValueObjects;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Services;

public class UserService(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext context = context;

    public async Task<bool> UserExistsAsync(string telegramName)
    {
        return await context.Users.AnyAsync(u => u.TgName == telegramName);
    }

    public async Task<Domain.Entities.User?> FindUserByTgAsync(TelegramInfo telegramInfo)
    {
        var dbUser = await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.Relations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Counsellor)
            .Where(u => u.TgName == telegramInfo.TgUsername)
            .FirstOrDefaultAsync();
        if (dbUser == null)
            return null;
        if (telegramInfo.TgId != 0 && dbUser.TgId != 0 && dbUser.TgId != telegramInfo.TgId)
            return null;
        return dbUser.ToDomainUser();
    }

    public async Task<Domain.Entities.User[]?> FindUsersByFullNameAsync(FullName fullName)
    {
        var userQuery = context.Users
        .Include(u => u.Student)
            .ThenInclude(s => s.Relations)
            .ThenInclude(r => r.Parent)
        .Include(u => u.Counsellor)
        .Where(u => (u.Student != null &&
                    u.Student.Name == fullName.Name &&
                    u.Student.Surname == fullName.Surname) ||
                   (u.Counsellor != null &&
                    u.Counsellor.Name == fullName.Name &&
                    u.Counsellor.Surname == fullName.Surname));

        if (!string.IsNullOrWhiteSpace(fullName.Patronymic))
        {
            userQuery = userQuery.Where(u =>
                (u.Student != null && u.Student.Patronymic != null &&
                 u.Student.Patronymic == fullName.Patronymic) ||
                (u.Counsellor != null && u.Counsellor.Patronymic != null &&
                 u.Counsellor.Patronymic == fullName.Patronymic));
        }

        var users = await userQuery.ToArrayAsync();
        var domainUsers = users.Select(u => u.ToDomainUser()).ToArray();
        return domainUsers.Length > 0 ? domainUsers : null;
    }

    public async Task<Domain.Entities.User[]?> FindUsersByTownAsync(string town)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.Relations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Counsellor)
            .Where(u => (u.Student != null && u.Student.City == town) || (u.Counsellor != null && u.Counsellor.City == town))
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public async Task<Domain.Entities.User[]?> FindUsersByClassAsync(int classNumber)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.Relations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Counsellor)
            .Where(u => u.Student != null && u.Student.CurrentClass == classNumber)
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public async Task<Domain.Entities.User[]?> FindUsersByGroupAsync(int group)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.Relations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Counsellor)
            .Where(u => (u.Student != null && u.Student.CurrentGroup == group) || (u.Counsellor != null && u.Counsellor.CurrentGroup == group))
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public Task AddUserAsync(Domain.Entities.User user)
    {
        throw new NotImplementedException();
    }

    public Task UpdateUserAsync(Domain.Entities.User user)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUserAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistUserAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }
}
