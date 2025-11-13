using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OzshBot.Infrastructure.Models
{
    [Table("parents")]
    public class Parent
    {
        [Key]
        [Column(name: "parent_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ParentId { get; set; }

        [Required]
        [Column(name: "name")]
        public required string Name { get; set; }
        [Required]
        [Column(name: "surname")]
        public required string Surname { get; set; }
        [Column(name: "patronymic")]
        public string? Patronymic { get; set; }

        [Column(name: "phone")]
        [Phone]
        public string? Phone { get; set; }

        public virtual List<ChildParent>? Relations { get; set; }
        [NotMapped]
        public List<Person>? Children => Relations?.Select(r => r.Child).ToList();
    }
}