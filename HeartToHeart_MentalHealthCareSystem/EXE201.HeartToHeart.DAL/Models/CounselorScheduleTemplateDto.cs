using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CounselorScheduleTemplateDto
    {
        [Required]
        [Range(0, 6)] // 0 = Sunday, 6 = Saturday
        public int DayOfWeek { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }
}
