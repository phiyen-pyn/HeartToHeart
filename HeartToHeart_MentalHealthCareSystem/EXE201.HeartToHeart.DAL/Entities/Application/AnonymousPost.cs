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
    public class AnonymousPost : AuditableEntityBase
    {
        public Guid? UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsReported { get; set; } = false;

        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
