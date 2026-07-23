using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CounselorAvailabilityDto
    {
        public Guid CounselorId { get; set; }
        public string? CounselorName { get; set; }
        public string? Specialization { get; set; }
        public decimal? HourlyRate { get; set; }
        public List<DateTime> AvailableSlots { get; set; } = new();
        public TimeSpan? CustomStartTime { get; set; }
        public TimeSpan? CustomEndTime { get; set; }
        public bool IsAvailableOnDate { get; set; } = true;
        public string? UnavailabilityReason { get; set; }
    }
}
