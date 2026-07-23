using EXE201.HeartToHeart.DAL.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class CounselorScheduleTemplate : EntityBase
    {
        [Required]
        public Guid CounselorId { get; set; }

        [Required]
        [Range(0, 6)] // 0 = Sunday, 6 = Saturday
        public int DayOfWeek { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public virtual Counselor Counselor { get; set; } = null!;
    }
}
