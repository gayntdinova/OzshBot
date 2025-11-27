using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Data;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Domain.ValueObjects;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Services;

public class DbRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext context = context;

    public async Task<Domain.Entities.User?> GetUserByTgAsync(TelegramInfo telegramInfo)
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

    public async Task<Domain.Entities.User[]?> GetUsersByFullNameAsync(FullName fullName)
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

    public async Task<Domain.Entities.User[]?> GetUsersByTownAsync(string town)
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

    public async Task<Domain.Entities.User[]?> GetUsersByClassAsync(int classNumber)
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

    public async Task<Domain.Entities.User?[]> GetUsersByGroupAsync(int group)
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

    public async Task AddUserAsync(Domain.Entities.User user)
    {
        var existingUser = await GetUserByTgAsync(user.TelegramInfo);
        if (existingUser != null) throw new InvalidOperationException("Пользователь с такими telegram данными уже существует");
        Student? dbStudent = StudentConverter.FromChildInfo(user.ChildInfo);
        Counsellor? dbCounsellor = CounsellorConverter.FromCounsellorInfo(user.CounsellorInfo);
        var dbUser = new User
        {
            UserId = user.Id,
            TgName = user.TelegramInfo.TgUsername,
            TgId = user.TelegramInfo.TgId,
            Role = user.Role,
            Student = dbStudent,
            Counsellor = dbCounsellor
        };
        await context.Users.AddAsync(dbUser);
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(Domain.Entities.User user)
    {
        var existingUser = await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.Relations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Counsellor)
            .Where(u => u.UserId == user.Id)
            .FirstOrDefaultAsync();
        if (existingUser == null) throw new InvalidOperationException("Нет пользователя с таким user_id");
        existingUser.TgName = user.TelegramInfo.TgUsername;
        existingUser.TgId = user.TelegramInfo.TgId;
        existingUser.Role = user.Role;
        switch (user.Role)
        {
            case Domain.Enums.Role.Child when user.ChildInfo != null:
                if (existingUser.Student != null)
                {
                    UpdateStudentFromChildInfo(existingUser.Student, user.ChildInfo);
                }
                else
                {
                    existingUser.Student = StudentConverter.FromChildInfo(user.ChildInfo, user);
                }
                break;

            case Domain.Enums.Role.Counsellor when user.CounsellorInfo != null:
                if (existingUser.Counsellor != null)
                {
                    UpdateCounsellorFromCounsellorInfo(existingUser.Counsellor, user.CounsellorInfo);
                }
                else
                {
                    existingUser.Counsellor = CounsellorConverter.FromCounsellorInfo(user.CounsellorInfo, user);
                }
                break;
        }
        await context.SaveChangesAsync();
    }

    private void UpdateStudentFromChildInfo(Student student, Domain.Entities.ChildInfo childInfo)
    {
        student.Name = childInfo.FullName.Name;
        student.Surname = childInfo.FullName.Surname;
        student.Patronymic = childInfo.FullName.Patronymic;
        student.City = childInfo.City;
        student.School = childInfo.EducationInfo.School;
        student.CurrentClass = childInfo.EducationInfo.Class;
        student.BirthDate = childInfo.Birthday;
        student.CurrentGroup = childInfo.Group;
        student.Email = childInfo.Email;
        student.Phone = childInfo.PhoneNumber;
    }

    private void UpdateCounsellorFromCounsellorInfo(Counsellor counsellor, Domain.Entities.CounsellorInfo counsellorInfo)
    {
        counsellor.Name = counsellorInfo.FullName.Name;
        counsellor.Surname = counsellorInfo.FullName.Surname;
        counsellor.Patronymic = counsellorInfo.FullName.Patronymic;
        counsellor.City = counsellorInfo.City;
        counsellor.BirthDate = counsellorInfo.Birthday;
        counsellor.CurrentGroup = counsellorInfo.Group;
        counsellor.Email = counsellorInfo.Email;
        counsellor.Phone = counsellorInfo.PhoneNumber;
    }


    public async Task DeleteUserAsync(TelegramInfo telegramInfo)
    {
        var user = await context.Users
            .Where(u => u.TgName == telegramInfo.TgUsername)
            .FirstOrDefaultAsync();
        if (user == null) throw new InvalidOperationException("Нет пользователя с такими данными telegram");
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}
