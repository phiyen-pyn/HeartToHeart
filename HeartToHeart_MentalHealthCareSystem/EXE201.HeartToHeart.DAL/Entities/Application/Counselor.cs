using EXE201.HeartToHeart.DAL.Entities.Base;
using EXE201.HeartToHeart.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class Counselor : AuditableEntityBase
    {
        public Guid UserId { get; set; }

        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Specialization { get; set; }

        [MaxLength(100)]
        public string? LicenseNumber { get; set; }

        public int? ExperienceYears { get; set; }

        public bool IsVerified { get; set; } = false;

        public decimal? HourlyRate { get; set; }

        public bool IsAvailable { get; set; } = true;

        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<CounselorAvailability> CustomAvailabilities { get; set; } = new List<CounselorAvailability>();
        public virtual ICollection<CounselorScheduleTemplate> ScheduleTemplates { get; set; } = new List<CounselorScheduleTemplate>();

    }
}
