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
    public class Report : AuditableEntityBase
    {
        public Guid PostId { get; set; }
        public Guid ReporterId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Reviewed, Resolved, Dismissed

        public virtual AnonymousPost Post { get; set; } = null!;
        public virtual ApplicationUser Reporter { get; set; } = null!;
    }
}
