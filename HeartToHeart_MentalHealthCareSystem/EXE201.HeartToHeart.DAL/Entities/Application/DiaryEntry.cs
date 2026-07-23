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
    public class DiaryEntry : AuditableEntityBase
    {
        public Guid UserId { get; set; }

        [MaxLength(150)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
    }
}
