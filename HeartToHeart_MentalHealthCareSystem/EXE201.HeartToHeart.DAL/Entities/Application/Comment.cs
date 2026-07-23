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
    public class Comment : AuditableEntityBase
    {
        public Guid PostId { get; set; }
        public Guid? UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public virtual AnonymousPost Post { get; set; } = null!;
        public virtual ApplicationUser? User { get; set; }
    }
}
