using EXE201.HeartToHeart.DAL.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class CounselorAvailability : EntityBase
    {
        [Required]
        public Guid CounselorId { get; set; }

        [Required]
        public DateTime Date { get; set; } // Specific date

        public bool IsAvailable { get; set; } = true;

        public TimeSpan? StartTime { get; set; } // Custom start time for this day

        public TimeSpan? EndTime { get; set; } // Custom end time for this day

        [MaxLength(500)]
        public string? Notes { get; set; } // Reason for unavailability

        public virtual Counselor Counselor { get; set; } = null!;
    }
}
