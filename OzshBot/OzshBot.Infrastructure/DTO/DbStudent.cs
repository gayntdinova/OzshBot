using System.ComponentModel.DataAnnotations;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.DTO
{
    public class DbStudent
    {
        public Guid UserId { get; set; }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string? Patronymic { get; set; }

        public string? City { get; set; }
        public string? School { get; set; }

        public DateOnly? BirthDate { get; set; }

        public int CurrentClass { get; set; }
        public int CurrentGroup { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        [Phone]
        public string? Phone { get; set; }

        public List<DbParent> Parents { get; }
    }
}