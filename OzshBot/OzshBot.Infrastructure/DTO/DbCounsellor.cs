namespace OzshBot.Infrastructure.DTO
{
    public class DbCounsellor
    {
        public Guid PersonId { get; set; }

        public string TgName { get; set; }
        public long TgId { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Patronymic { get; set; }

        public DateOnly? BirthDate { get; set; }

        public int CurrentGroup { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}