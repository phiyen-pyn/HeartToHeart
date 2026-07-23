using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class SetCounselorAvailabilityDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
