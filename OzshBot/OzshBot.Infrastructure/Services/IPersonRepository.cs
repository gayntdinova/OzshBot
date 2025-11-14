using OzshBot.Infrastructure.DTO;

namespace OzshBot.Infrastructure.Services;

public interface IPersonRepository
{
    public DbRequestResult FindByTgName(string tgname);
    public DbRequestResult FindByName(string? name, string? surname, string? patronymic);
    public DbRequestResult FindByCity(string city);
    public DbRequestResult FindBySchool(string school);

    public DbRequestResult GetGroupList(int group);
    public DbRequestResult GetClassList(int classNumber);

    public void AddStudent(DbStudent student);
    public void ChangeStudent(DbStudent newStudentData);
    public void AddCounsellor(DbCounsellor counsellor);
    public void ChangeCounsellor(DbCounsellor newCounsellorData);

    public void Promote(string tgname);
    public void Downgrade(string tgname);
}
