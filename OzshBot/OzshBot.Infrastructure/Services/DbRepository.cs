using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Data;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Domain.ValueObjects;
using OzshBot.Infrastructure.Models;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;

namespace OzshBot.Infrastructure.Services;

public class DbRepository(AppDbContext context) : IUserRepository, ISessionRepository
{
    private readonly AppDbContext context = context;
    private readonly SessionsRepository sessionsRepository = new(context);

    public async Task<Domain.Entities.User?> GetUserByTgAsync(TelegramInfo telegramInfo)
    {
        var dbUser = await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u => u.TgName == telegramInfo.TgUsername)
            .FirstOrDefaultAsync();
        if (dbUser == null)
            return null;
        if (telegramInfo.TgId != null && dbUser.TgId != 0 && dbUser.TgId != telegramInfo.TgId)
            return null;
        return dbUser.ToDomainUser();
    }

    public async Task<Domain.Entities.User?> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u => (u.Student != null && u.Student.Phone == phoneNumber) || (u.Counsellor != null && u.Counsellor.Phone == phoneNumber))
            .Select(u => u.ToDomainUser())
            .FirstOrDefaultAsync();
    }

    public async Task<Domain.Entities.User?> GetUserByIdAsync(Guid userId)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u => u.UserId == userId)
            .Select(u => u.ToDomainUser())
            .FirstOrDefaultAsync();
    }

    public async Task<Domain.Entities.User[]?> GetUsersByFullNameAsync(NameSearch fullName)
    {
        if (fullName.Name == null && fullName.Surname == null && fullName.Patronymic == null)
            return [];

        var userQuery = context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u =>
                (string.IsNullOrWhiteSpace(fullName.Name) ||
                 (u.Student != null && u.Student.Name == fullName.Name) ||
                 (u.Counsellor != null && u.Counsellor.Name == fullName.Name)) &&
                (string.IsNullOrWhiteSpace(fullName.Surname) ||
                 (u.Student != null && u.Student.Surname == fullName.Surname) ||
                 (u.Counsellor != null && u.Counsellor.Surname == fullName.Surname)) &&
                (string.IsNullOrWhiteSpace(fullName.Patronymic) ||
                 (u.Student != null && u.Student.Patronymic == fullName.Patronymic) ||
                 (u.Counsellor != null && u.Counsellor.Patronymic == fullName.Patronymic))
            );

        var users = await userQuery.ToArrayAsync();
        var domainUsers = users.Select(u => u.ToDomainUser()).ToArray();
        return domainUsers;
    }

    public async Task<Domain.Entities.User[]?> GetUsersByNameAsync(string Name)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session) 
            .Where(u => (u.Student != null &&
                        u.Student.Name == Name) ||
                    (u.Counsellor != null &&
                        u.Counsellor.Name == Name))
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public async Task<Domain.Entities.User[]?> GetUsersBySurnameAsync(string Surname)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u => (u.Student != null &&
                        u.Student.Surname == Surname) ||
                    (u.Counsellor != null &&
                        u.Counsellor.Surname == Surname))
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public async Task<Domain.Entities.User[]?> GetUsersByCityAsync(string town)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u => (u.Student != null && u.Student.City == town) || (u.Counsellor != null && u.Counsellor.City == town))
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public async Task<Domain.Entities.User[]?> GetUsersByClassAsync(int classNumber)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u => u.Student != null && u.Student.CurrentClass == classNumber)
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public async Task<Domain.Entities.User?[]> GetUsersByGroupAsync(int group)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u => (u.Student != null && u.Student.CurrentGroup == group) || (u.Counsellor != null && u.Counsellor.CurrentGroup == group))
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public async Task<Domain.Entities.User[]?> GetUsersBySchoolAsync(string school)
    {
        return await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Student)
                .ThenInclude(s => s.SessionRelations)
                .ThenInclude(sr => sr.Session)
            .Include(u => u.Counsellor)
                .ThenInclude(c => c.SessionRelations)
                .ThenInclude(csr => csr.Session)
            .Where(u => u.Student != null && u.Student.School == school)
            .Select(u => u.ToDomainUser())
            .ToArrayAsync();
    }

    public async Task AddUserAsync(Domain.Entities.User user)
    {
        var existingUser = await GetUserByPhoneNumberAsync(user.PhoneNumber);
        if (existingUser != null) throw new InvalidOperationException("Пользователь с таким номером телефона уже существует");

        var dbUser = UserConverter.FromDomainUser(user);
        await context.Users.AddAsync(dbUser);
        if (user.ChildInfo != null && user.ChildInfo.ContactPeople.Count != 0)
        {
            await UpdateContactPeopleAsync(dbUser.Student, user.ChildInfo.ContactPeople);
        }
        if (user.ChildInfo != null && user.ChildInfo.Sessions.Count != 0)
        {
            await UpdateStudentSessionsAsync(dbUser.Student, user.ChildInfo.Sessions);
        }
        if (user.CounsellorInfo != null && user.CounsellorInfo.Sessions.Count != 0)
        {
            await UpdateCounsellorSessionsAsync(dbUser.Counsellor, user.CounsellorInfo.Sessions);
        }
        await context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(Domain.Entities.User user)
    {
        var existingUser = await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Counsellor)
            .Where(u => u.UserId == user.Id)
            .FirstOrDefaultAsync();
        if (existingUser == null) throw new InvalidOperationException("Нет пользователя с таким user_id");
        existingUser.TgName = user.TelegramInfo.TgUsername;
        existingUser.TgId = user.TelegramInfo.TgId;
        existingUser.Role = user.Role;
        if (existingUser.Counsellor != null)
        {
            existingUser.Counsellor.Name = user.FullName.Name;
            existingUser.Counsellor.Surname = user.FullName.Surname;
            existingUser.Counsellor.Patronymic = user.FullName.Patronymic;
            existingUser.Counsellor.City = user.City;
            existingUser.Counsellor.Email = user.Email;
            existingUser.Counsellor.Phone = user.PhoneNumber;
            existingUser.Counsellor.BirthDate = user.Birthday ?? default;
        }
        if (existingUser.Student != null)
        {
            existingUser.Student.Name = user.FullName.Name;
            existingUser.Student.Surname = user.FullName.Surname;
            existingUser.Student.Patronymic = user.FullName.Patronymic;
            existingUser.Student.City = user.City;
            existingUser.Student.Email = user.Email;
            existingUser.Student.Phone = user.PhoneNumber;
            existingUser.Student.BirthDate = user.Birthday ?? default;
        }
        switch (user.Role)
        {
            case Role.Child when user.ChildInfo != null:
                if (existingUser.Student != null)
                {
                    existingUser.Student.UpdateFromChildInfo(user.ChildInfo);
                    await UpdateContactPeopleAsync(existingUser.Student, user.ChildInfo.ContactPeople);
                    await UpdateStudentSessionsAsync(existingUser.Student, user.ChildInfo.Sessions);
                }
                break;

            case Role.Counsellor when user.CounsellorInfo != null:
                if (existingUser.Counsellor != null)
                {
                    existingUser.Counsellor.UpdateFromCounsellorInfo(user.CounsellorInfo);
                    await UpdateCounsellorSessionsAsync(existingUser.Counsellor, user.CounsellorInfo.Sessions);
                }
                break;
        }
        await context.SaveChangesAsync();
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

    private async Task UpdateContactPeopleAsync(Student student, HashSet<ContactPerson> contactPeople)
    {
        if (student.ParentRelations != null && student.ParentRelations.Any())
        {
            context.ChildrenParents.RemoveRange(student.ParentRelations);
        }
        if (contactPeople != null && contactPeople.Count != 0)
        {
            student.ParentRelations ??= new List<ChildParent>();
            foreach (var contactPerson in contactPeople)
            {
                var parent = await FindOrCreateParentAsync(contactPerson);
                student.ParentRelations.Add(new ChildParent
                {
                    ChildId = student.StudentId,
                    ParentId = parent.ParentId
                });
            }
        }
    }

    private async Task UpdateStudentSessionsAsync(Student student, HashSet<Domain.Entities.Session> sessions)
    {
        if (student.SessionRelations != null && student.SessionRelations.Any())
        {
            context.StudentsSessions.RemoveRange(student.SessionRelations);
        }
        if (sessions != null && sessions.Count != 0)
        {
            student.SessionRelations ??= new List<StudentSession>();
            foreach (var session in sessions)
            {
                student.SessionRelations.Add(new StudentSession
                {
                    StudentId = student.StudentId,
                    SessionId = session.Id
                });
            }
        }
    }

    private async Task UpdateCounsellorSessionsAsync(Counsellor counsellor, HashSet<Domain.Entities.Session> sessions)
    {
        if (counsellor.SessionRelations != null && counsellor.SessionRelations.Any())
        {
            context.CounsellorsSessions.RemoveRange(counsellor.SessionRelations);
        }
        if (sessions != null && sessions.Count != 0)
        {
            counsellor.SessionRelations ??= new List<CounsellorSession>();
            foreach (var session in sessions)
            {
                counsellor.SessionRelations.Add(new CounsellorSession
                {
                    CounsellorId = counsellor.CounsellorId,
                    SessionId = session.Id
                });
            }
        }
    }

    private async Task<Parent> FindOrCreateParentAsync(ContactPerson contactPerson)
    {
        var existingParent = await context.Parents
            .FirstOrDefaultAsync(p => p.Phone == contactPerson.PhoneNumber);

        if (existingParent != null)
        {
            existingParent.Name = contactPerson.FullName.Name;
            existingParent.Surname = contactPerson.FullName.Surname;
            existingParent.Patronymic = contactPerson.FullName.Patronymic;
            return existingParent;
        }
        else
        {
            var newParent = new Parent
            {
                ParentId = Guid.NewGuid(),
                Name = contactPerson.FullName.Name,
                Surname = contactPerson.FullName.Surname,
                Patronymic = contactPerson.FullName.Patronymic,
                Phone = contactPerson.PhoneNumber
            };
            await context.Parents.AddAsync(newParent);
            return newParent;
        }
    }

    public async Task DeleteUserAsync(string phoneNumber)
    {
        var user = await context.Users
            .Include(u => u.Student)
                .ThenInclude(s => s.ParentRelations)
                .ThenInclude(r => r.Parent)
            .Include(u => u.Counsellor)
            .Where(u => (u.Student != null && u.Student.Phone == phoneNumber) || (u.Counsellor != null && u.Counsellor.Phone == phoneNumber))
            .FirstOrDefaultAsync();
        if (user == null) throw new InvalidOperationException("Нет пользователя с таким номером телефона");
        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }

    public async Task AddSessionAsync(Domain.Entities.Session session) => await sessionsRepository.AddSessionAsync(session);

    public async Task UpdateSessionAsync(Domain.Entities.Session session) => await sessionsRepository.UpdateSessionAsync(session);

    public async Task<Domain.Entities.Session?> GetSessionByDatesAsync(SessionDates sessionDates) => await sessionsRepository.GetSessionByDatesAsync(sessionDates);

    public async Task<Domain.Entities.Session?> GetSessionByIdAsync(Guid sessionId) => await sessionsRepository.GetSessionByIdAsync(sessionId);

    public async Task<Domain.Entities.Session[]?> GetLastSessionsAsync(int numberOfSessions) => await sessionsRepository.GetLastSessionsAsync(numberOfSessions);

    public async Task<Domain.Entities.Session[]?> GetAllSessions() => await sessionsRepository.GetAllSessions();
}
