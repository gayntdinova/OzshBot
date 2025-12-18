using System.ComponentModel.DataAnnotations.Schema;

namespace OzshBot.Infrastructure.Models;

[Table("students_sessions")]
public class StudentSession
{
    [Column("student_id")]
    public Guid StudentId { get; set; }

    [ForeignKey("StudentId")]
    public virtual Student Student { get; set; }

    [Column("session_id")]
    public Guid SessionId { get; set; }

    [ForeignKey("SessionId")]
    public virtual Session Session { get; set; }
}


[Table("counsellors_sessions")]
public class CounsellorSession
{
    [Column("counsellor_id")]
    public Guid CounsellorId { get; set; }
    
    [ForeignKey("CounsellorId")]
    public virtual Counsellor Counsellor { get; set; }
    
    [Column("session_id")]
    public Guid SessionId { get; set; }
    
    [ForeignKey("SessionId")]
    public virtual Session Session { get; set; }
}