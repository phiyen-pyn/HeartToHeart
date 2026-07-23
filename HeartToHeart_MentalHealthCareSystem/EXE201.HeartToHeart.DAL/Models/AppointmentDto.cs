using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class AppointmentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CounselorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }
        public int DurationMinutes { get; set; } = 60;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? CounselorName { get; set; }
        public string? CounselorSpecialization { get; set; }

        // NEW: Google Calendar integration
        public string? GoogleCalendarEventId { get; set; }
        public string? GoogleMeetLink { get; set; }
    }
}
