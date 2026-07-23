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
    public class EmotionTrack : AuditableEntityBase
    {
        public Guid UserId { get; set; }

        [MaxLength(50)]
        public string? Emotion { get; set; }

        [MaxLength(255)]
        public string? Note { get; set; }

        public int? IntensityLevel { get; set; } // 1-10 scale

        public virtual ApplicationUser User { get; set; } = null!;
    }
}
