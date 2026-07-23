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
    public class Appointment : AuditableEntityBase
    {
        public Guid UserId { get; set; }
        public Guid CounselorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Reason { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled

        public string? Notes { get; set; }
        public int DurationMinutes { get; set; } = 60;

        // NEW: Google Calendar integration
        [MaxLength(100)]
        public string? GoogleCalendarEventId { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Counselor Counselor { get; set; } = null!;
        public virtual ICollection<AppointmentHistory> HistoryChanges { get; set; } = new List<AppointmentHistory>();
    }
}
