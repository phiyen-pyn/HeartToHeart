using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class ValidateAppointmentTimeDto
    {
        [Required]
        public Guid CounselorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Range(30, 180)]
        public int DurationMinutes { get; set; } = 60;
    }
}
