using OzshBot.Infrastructure.DTO;

namespace OzshBot.Infrastructure.Services
{
    public interface IPersonRepository
    {
        public ResultDTO FindByTgName(string tgname);
        public ResultDTO FindByName(string? name, string? surname, string? patronymic);
        public ResultDTO FindByCity(string city);
        public ResultDTO FindBySchool(string school);

        public ResultDTO GetGroupList(int group);
        public ResultDTO GetClassList(int classNumber);

        public void AddStudent(DbStudent student);
        public void ChangeStudent(DbStudent newStudentData);
        public void AddCounsellor(DbCounsellor counsellor);
        public void ChangeCounsellor(DbCounsellor newCounsellorData);

        public void Promote(string tgname);
        public void Downgrade(string tgname);
    }
}