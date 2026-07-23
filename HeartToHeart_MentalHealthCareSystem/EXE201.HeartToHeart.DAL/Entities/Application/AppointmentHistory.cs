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
    public class AppointmentHistory : EntityBase
    {
        [Required]
        public Guid AppointmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // Created, Updated, Cancelled, Completed, etc.

        [Required]
        [MaxLength(50)]
        public string OldStatus { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string NewStatus { get; set; } = string.Empty;

        public DateTime? OldAppointmentDate { get; set; }

        public DateTime? NewAppointmentDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public Guid ChangedByUserId { get; set; }

        public virtual Appointment Appointment { get; set; } = null!;
        public virtual ApplicationUser ChangedByUser { get; set; } = null!;
    }
}
