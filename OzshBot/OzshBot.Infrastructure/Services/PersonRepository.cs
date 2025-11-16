using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OzshBot.Infrastructure.Data;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Services;


public class PersonRepository
{
    private readonly AppDbContext context;

    public PersonRepository()
    {
        context = AppDbContextFactory.CreateContext();
    }

    public async Task<Person?> FindByTgName(string tgname) => await context.People.Where(p => p.User.TgName == tgname).Select(p => p).FirstOrDefaultAsync();
    
    private static bool FullNameSelector(Person person, string? name, string? surname, string? patronymic)
    {
        var nameEquals = name is null || person.Name == name;
        var surnameEquals = surname is null || person.Surname == surname;
        var patronymicEquals = patronymic is null || person.Patronymic == patronymic;
        return nameEquals && surnameEquals && patronymicEquals;
    }

    public async Task<List<Person>> FindByName(string? name, string? surname, string? patronymic) => await context.People.Where(p => FullNameSelector(p, name, surname, patronymic)).Select(p => p).ToListAsync();
    public async Task<List<Person>> FindByCity(string city) => await context.People.Where(p => p.City == city).Select(p => p).ToListAsync();
    public async Task<List<Person>> FindBySchool(string school) => await context.People.Where(p => p.School == school).Select(p => p).ToListAsync();

    public async Task<List<Person>> GetGroupList(int group) => await context.People.Where(p => p.CurrentGroup == group).Select(p => p).ToListAsync();
    public async Task<List<Person>> GetClassList(int classNumber) => await context.People.Where(p => p.CurrentClass == classNumber).Select(p => p).ToListAsync();
}