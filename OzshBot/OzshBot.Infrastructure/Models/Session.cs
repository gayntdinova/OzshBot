using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Infrastructure.Models;

[Table("sessions")]
public class Session
{
    [Key]
    [Column(name: "session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SessionId { get; set; }

    [Required]
    [Column(name: "start_date")]
    public DateOnly StartDate { get; set; }

    [Required]
    [Column(name: "end_date")]
    public DateOnly EndDate { get; set; }
}

public static class SessionConverter
{
    public static Domain.Entities.Session ToDomainSession(this Session session)
    {
        return new Domain.Entities.Session
        {
            Id = session.SessionId,
            SessionDates = new SessionDates(session.StartDate, session.EndDate)
        };
    }
    
    public static Session FromDomainSession (Domain.Entities.Session session)
    {
        return new Session
        {
            SessionId = session.Id,
            StartDate = session.SessionDates.StartDate,
            EndDate = session.SessionDates.EndDate
        };
    }
}
